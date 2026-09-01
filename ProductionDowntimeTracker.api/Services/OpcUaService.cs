using Microsoft.Extensions.Options;
using ProductionDowntimeTracker.api.DTOs;
using ProductionDowntimeTracker.api.Options;
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Client;

namespace ProductionDowntimeTracker.api.Services
{
    public class OpcUaService : IOpcUaService, IAsyncDisposable
    {
        private readonly OpcUaOptions _options;
        private readonly ITelemetryContext _telemetry;

        private Opc.Ua.Client.ISession? _session;
        private readonly SemaphoreSlim _sessionLock = new(1, 1);

        public OpcUaService(IOptions<OpcUaOptions> options, ITelemetryContext telemetry)
        {
            _options = options.Value;
            _telemetry = telemetry;
        }

        private async Task<Opc.Ua.Client.ISession> CreateSessionAsync()
        {

            string pkiRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ProductionDowntimeTracker",
                "pki");

            var applicationCertificates = new CertificateIdentifierCollection
            {
                new CertificateIdentifier
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = pkiRoot,
                    SubjectName = "CN=ProductionDowntimeTracker",
                    CertificateType = ObjectTypeIds.RsaSha256ApplicationCertificateType
                }
            };

            var application = new ApplicationInstance(_telemetry)
            {
                ApplicationName = "ProductionDowntimeTracker",
                ApplicationType = ApplicationType.Client
            };

            ApplicationConfiguration configuration = await application
                .Build(
                    "urn:localhost:ProductionDowntimeTracker",
                    "urn:ProductionDowntimeTracker")
                .AsClient()
                .AddSecurityConfiguration(applicationCertificates, pkiRoot)
                .SetAutoAcceptUntrustedCertificates(true)
                .CreateAsync();

            bool certificateIsValid =
                await application.CheckApplicationInstanceCertificatesAsync(true);

            if (!certificateIsValid)
            {
                throw new InvalidOperationException(
                    "Nepodařilo se vytvořit nebo ověřit certifikát OPC UA klienta.");
            }

            EndpointDescription endpointDescription =
                await CoreClientUtils.SelectEndpointAsync(
                    configuration,
                    _options.EndpointUrl,
                    useSecurity: true,
                    _telemetry)
                ?? throw new InvalidOperationException(
                    "OPC UA server nenabídl žádný použitelný endpoint.");

            var configuredEndpoint = new ConfiguredEndpoint(
                null,
                endpointDescription,
                EndpointConfiguration.Create(configuration));

            var sessionFactory = new DefaultSessionFactory(_telemetry);

            Opc.Ua.Client.ISession session =
                await sessionFactory.CreateAsync(
                configuration,
                configuredEndpoint,
                updateBeforeConnect: false,
                checkDomain: false,
                sessionName: "ProductionDowntimeTrackerSession",
                sessionTimeout: 60_000,
                identity: new UserIdentity(),
                preferredLocales: null);

            if (!session.Connected)
            {
                session.Dispose();

                throw new InvalidOperationException(
                    "Nepodařilo se otevřít OPC UA session.");
            }

            return session;
        }

        private async Task<Opc.Ua.Client.ISession> GetOrCreateSessionAsync()
        {
            await _sessionLock.WaitAsync();

            try
            {
                if (_session is not null && _session.Connected)
                {
                    return _session;
                }

                _session?.Dispose();
                _session = await CreateSessionAsync();

                return _session;
            }
            finally
            {
                _sessionLock.Release();
            }
        }

        private async Task ResetSessionAsync(
            Opc.Ua.Client.ISession failedSession)
        {
            await _sessionLock.WaitAsync();

            try
            {
                if (!ReferenceEquals(_session, failedSession))
                {
                    return;
                }

                try
                {
                    _session?.Dispose();
                }
                catch
                {
                    // Nefunkční session se nemusí podařit korektně uvolnit.
                }

                _session = null;
            }
            finally
            {
                _sessionLock.Release();
            }
        }

        public async Task<OpcUaStatusDto> ReadStatusAsync()
        {
            Opc.Ua.Client.ISession session =
                await GetOrCreateSessionAsync();

            try
            {
                NodeId machineRunningNodeId = ExpandedNodeId.Parse(
                    _options.MachineRunningNodeId,
                    session.NamespaceUris);

                NodeId faultActiveNodeId = ExpandedNodeId.Parse(
                    _options.FaultActiveNodeId,
                    session.NamespaceUris);

                NodeId producedCyclesNodeId = ExpandedNodeId.Parse(
                    _options.ProducedCyclesNodeId,
                    session.NamespaceUris);

                var nodesToRead = new ReadValueIdCollection
                {
                    new ReadValueId
                    {
                        NodeId = machineRunningNodeId,
                        AttributeId = Attributes.Value
                    },
                    new ReadValueId
                    {
                        NodeId = faultActiveNodeId,
                        AttributeId = Attributes.Value
                    },
                    new ReadValueId
                    {
                        NodeId = producedCyclesNodeId,
                        AttributeId = Attributes.Value
                    }
                };

                ReadResponse response = await session.ReadAsync(
                    null,
                    0,
                    TimestampsToReturn.Neither,
                    nodesToRead,
                    CancellationToken.None);

                if (response.Results.Count != nodesToRead.Count)
                {
                    throw new InvalidOperationException(
                        "OPC UA server nevrátil očekávaný počet hodnot.");
                }

                foreach (DataValue result in response.Results)
                {
                    if (StatusCode.IsBad(result.StatusCode))
                    {
                        throw new InvalidOperationException(
                            $"Čtení OPC UA hodnoty selhalo: {result.StatusCode}");
                    }
                }

                if (response.Results[0].Value is not bool machineRunning ||
                    response.Results[1].Value is not bool faultActive ||
                    response.Results[2].Value is not uint producedCycles)
                {
                    throw new InvalidOperationException(
                        "OPC UA server vrátil neočekávané datové typy.");
                }

                var status = new OpcUaStatusDto
                {
                    MachineRunning = machineRunning,
                    FaultActive = faultActive,
                    ProducedCycles = producedCycles
                };

                return status;
            }

            catch
            {
                await ResetSessionAsync(session);
                throw;
            }
        }
        public async ValueTask DisposeAsync()
        {
            await _sessionLock.WaitAsync();

            try
            {
                if (_session is not null)
                {
                    try
                    {
                        if (_session.Connected)
                        {
                            await _session.CloseAsync(
                                timeout: 2_000,
                                closeChannel: true);
                        }
                    }
                    catch
                    {
                        // Při vypínání aplikace už spojení nemusí být dostupné.
                    }
                    finally
                    {
                        try
                        {
                            _session.Dispose();
                        }
                        catch
                        {
                            // Uvolnění poškozené session může také selhat.
                        }

                        _session = null;
                    }
                }
            }
            finally
            {
                _sessionLock.Release();
                _sessionLock.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}