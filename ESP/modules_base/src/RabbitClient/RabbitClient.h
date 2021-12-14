
void reconnect();

void Configure_Rabbit(const char *mqtt_ip);

bool IsConnected();

void PublishMessage(const char *topic, const char *payload);