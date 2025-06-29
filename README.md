# AbxClient
This project is a C# client for the ABX Mock Exchange Server. It connects to a TCP server running locally, requests a stream of stock ticker packets, ensures all packets are received in order (with no missing sequences), and saves the result to a JSON file.

1. Prerequisites
-> .NET SDK 6.0 or later
-> Node.js v16.17.0 or later
-> Git (to clone the repository)

2. Clone the Repository

3. Start the ABX Server
   -> cd ABX-Mock-Server
   -> node main.js
The server will start on TCP port 3000. Leave it running in the background.

4. Run the C# Client
   -> cd AbxExchangeClient
   -> dotnet run
   
This will:
- Request all packets from the server
- Detect missing sequences
- Request missing packets individually
- Generate output.json in the project root

