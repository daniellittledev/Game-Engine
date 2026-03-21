using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

//using Microsoft.DirectX.DirectPlay;
//using DP = Microsoft.DirectX.DirectPlay;

using System.Net.Sockets;
using System.Net;
using Sockets = System.Net.Sockets;

using System.Threading;


namespace EngineX.Network
{
    public class Networking
    {

        struct Player
        {
            //Socket of the client
            public EndPoint endPoint;
            //Name
            public string name;
        }

        struct ACK
        {

            public int ID;
            public bool success;

        }

        struct GameDetails
        {

            public string GameName;
            public string GameType;
            public int currentPlayers;
            public int maxPlayers;
            public int version;
            public string map;
            
        }


        public enum Types
        { 
            FindHosts,
            Join,
            Leave,
            UpdateObject,
            CreateObject,
            RemoveObject,
            TextMessage,
            Beat
        }



        bool isHost;
        bool inGame;

        Socket netSocket;
        List<Player> players;
        //Queue<netMessage> messages;
        //IPEndPoint Server;
        Thread listenTread;

        double sendTimeOut = 1.0d;
        double resendTime = 0.1d;

        GameDetails gameDetail;

        int[] byteValues;
        const byte scan = (byte)1;

        //string playerName;

        public Networking(int Port, string playerName)
        {
            players = new List<Player>(16);

            Player current = new Player();
            current.name = playerName;
            players[0] = current;

            byteValues[0] = 1;
            byteValues[1] = 2;
            byteValues[2] = 4;
            byteValues[3] = 8;
            byteValues[4] = 16;
            byteValues[5] = 32;
            byteValues[6] = 64;
            byteValues[7] = 128;
            byteValues[8] = 256;
  
            netSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void HostGame()//GameDetails gameDetails) 
        {
            gameDetail = gameDetail;
            //Timers.HiResTimer.GetAbsoluteTime 
            inGame = true;
            isHost = true;

            netSocket.Listen(10);
        }

        public void JoinGame() 
        {

            netSocket.Listen(10);

            // Broadcast
            byte[] recievedData;

            //System.IO.BinaryWriter writer = new System.IO.BinaryWriter(

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            // Change to send Wait
            Send(false, false, scan, encoding.GetBytes(players[0].name));


        }

        public void FindGames() 
        {

            netSocket.Listen(10);
        }

        public void LeaveGame() 
        {
        
            // If server
            // Send Update Game Over
            // Else
            // Send Update Leaving Game

            // Close Socket
            // Set inGame
        
        }

        //Updates (Updateiee is info)
        //Players
        //PlayerDetails

        //Data can be Reliable\

        private byte[] ConstructPacket(byte typeCode, byte[] data, params bool[] option)
        {

            byte[] result = new byte[data.Length + 2];
            
            int byteValue = 0;
            for (int i = 0; i < option.Length; i++)
			{
                if (option[i] == true)
                {
			        byteValue += byteValues[i];
                }
			}
            result[0] = (byte)byteValue;

            result[1] = typeCode;
            Array.Copy(data, 0, result, 2, data.Length);
            return result;

        }

        private void Send(bool isUpdate, bool reliable, byte typeCode, byte[] data)
        {

            data = ConstructPacket(typeCode, data, isUpdate, reliable);

            if (isHost)
            {
                netSocket.Send(data, data.Length, SocketFlags.Broadcast); 
            }
            else
            {
                //netSocket.SendTo(data, data.Length, SocketFlags.None, Server);
            }
        }

        private bool Send(bool isUpdate, byte[] data)
        {

            return true;
        }

        //public void MakeUpdate(string type, object data)
        //{

        //    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter;
        //    formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

        //    System.IO.MemoryStream stream = new System.IO.MemoryStream();
        //    formatter.Serialize(stream, data);

        //    byte[] info = stream.ToArray();

        //    if (isServer)
        //    {
        //        netSocket.Send(info, info.Length, SocketFlags.Broadcast);
        //    }
        //    else
        //    {
        //        netSocket.SendTo(info, info.Length, SocketFlags.None, Server);
        //    }

        //}



        public void Listen()
        {
            if (inGame)
            {
                if (isHost)
                {
                    ListenHost();
                }
                else
                { 
                    ListenClient();
                }
            }
            //netSocket.BeginReceive(
            //netSocket.Receive(
            //netSocket.ReceiveAsync(
            
            //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            //EndPoint tmpRemote = (EndPoint)sender;

            //netSocket.ReceiveFrom(data, ref tmpRemote);
            //netSocket.Receive();
        }

        private void ListenHost()
        {

            byte[] recievedData = null;
            netSocket.Receive(recievedData);
            // netSocket.beg
            bool[] options = new bool[8];
            byte typeCode;
            byte[] data = new byte[recievedData.Length - 2];


            BitArray op = new BitArray(recievedData[0]);
            op.CopyTo(options, 0);

            typeCode = recievedData[1];

            Array.Copy(recievedData, 2,  data, 0, data.Length);

            if (recievedData != null)
            {
                // If Not an update message
                if (options[0] == false)
                {

                    switch (typeCode)
                    {
                        case scan:

                            // Send Back...


                            break;
                    }

                }
            }
        
            // Get Request
            // Join Request
            // Leave Request
        
        }

        private void ListenClient()
        { }

    }
}
