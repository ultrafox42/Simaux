using System;
using System.Threading;
using CTrue.FsConnect;
using CTrue.FsConnect.Managers;
using DebugWindow;

namespace MsfsConnect
{
    public class FlightSimulator
    {
        public bool IsConnected { get { return m_FsConnect.Connected; } }
        public PlaneInfoResponse PlaneInfo { get { return m_AircraftManager.Get(); } }
        public FlightSimulator()
        {
            m_FsConnect = new FsConnect();
        }
        public bool Connect()
        {
            try
            {
                //m_FsConnect.SimConnectFileLocation = SimConnectFileLocation.Local;
                m_FsConnect.Connect("MsfsConnect", "localhost", 500, SimConnectProtocol.Ipv4);
                m_FsConnect.ConnectionChanged += OnFsConnectOnConnectionChanged;
                m_FsConnect.RegisterDataDefinition<PlaneInfoResponse>(Definitions.PlaneInfo);
                m_AircraftManager = new AircraftManager<PlaneInfoResponse>(m_FsConnect, Definitions.PlaneInfo, Requests.AircraftManager);
                m_AircraftManager.Updated += OnAircraftManagerUpdated;
                return m_FsConnect.Connected;
            }
            catch (Exception ex)
            {
                Log.Trace(ex.Message);
                return false;
            }
        }
        public void Disconnect()
        {
            m_AircraftManager?.Dispose();
            if (m_FsConnect.Connected)
                m_FsConnect.Disconnect();
        }
        private void OnFsConnectOnConnectionChanged(object sender, bool args)
        {
            Log.Trace(m_FsConnect.Connected ? "Connected to Flight Simulator" : "Disconnected from Flight Simulator");
            if (m_FsConnect.Connected)
            {
                Log.Trace($"Application:        \t {m_FsConnect.ConnectionInfo.ApplicationName}");
                Log.Trace($"Application Version:\t {m_FsConnect.ConnectionInfo.ApplicationVersion}");
                Log.Trace($"Application Build:  \t {m_FsConnect.ConnectionInfo.ApplicationBuild}");
                Log.Trace($"SimConnect Version: \t {m_FsConnect.ConnectionInfo.SimConnectVersion}");
                Log.Trace($"SimConnect Build:   \t {m_FsConnect.ConnectionInfo.SimConnectBuild}");

                m_ConnectedResetEvent.Set();
            }
        }
        private void OnAircraftManagerUpdated(object sender, AircraftInfoUpdatedEventArgs<PlaneInfoResponse> args)
        {
            Log.Trace(args.AircraftInfo.ToString());
        }
        private FsConnect m_FsConnect = null;
        private AircraftManager<PlaneInfoResponse> m_AircraftManager = null;
        
        private readonly AutoResetEvent m_ConnectedResetEvent = new AutoResetEvent(false);
    }
}
