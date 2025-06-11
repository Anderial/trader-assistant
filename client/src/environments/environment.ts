export const environment = {
  production: false,
  apiUrl: 'https://localhost:7216/api',
  wsUrl: 'wss://localhost:7216/ws', // для будущих WebSocket соединений
  enableLogging: true,
  realTimeUpdateInterval: 5000, // интервал обновления в миллисекундах
  notificationDuration: 3000 // длительность показа уведомлений
}; 