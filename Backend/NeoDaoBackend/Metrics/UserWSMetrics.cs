using System.Diagnostics.Metrics;

namespace NeoDaoBackend.Metrics;

public class UserWSMetrics
{
    private readonly Gauge<int> _activeConnectionGauge;
    private readonly Counter<int> _inputWebSocketMessage;
    private readonly Counter<int> _outputWebSocketMessage;

    public UserWSMetrics(IMeterFactory metricFactory)
    {
        var meter = metricFactory.Create("BackendNeoDao.WebSocket");
        _activeConnectionGauge = meter.CreateGauge<int>("backend_neo_dao.websocket_active_connection");
        _activeConnectionGauge.Record(0);
        _inputWebSocketMessage = meter.CreateCounter<int>("backend_neo_dao.input_web_socket_message");
        _inputWebSocketMessage.Add(0);
        _outputWebSocketMessage = meter.CreateCounter<int>("backend_neo_dao.output_web_socket_message");
        _outputWebSocketMessage.Add(0);
    }

    public void WebSocketConnectionUpdate(int count)
    {
        _activeConnectionGauge.Record(count);
    }
    public void AddOutputWebSocketMessage()
    {
        _outputWebSocketMessage.Add(1);
    }
    public void AddInputWebSocketMessage()
    {
        _inputWebSocketMessage.Add(1);
    }
}
