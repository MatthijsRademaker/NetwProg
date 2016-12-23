using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MultiClientServer
{
    class Program
    {
        static public int MijnPoort;

        static public Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();

        static public Dictionary<int, int> RoutingTable = new Dictionary<int, int>();

        static public Connection a;

        static public String[] args;

        static bool b;

        static public object Lock = new object();

        static public Thread[] threads = new Thread[4];

        static void Main(string[] arg)
        {
            Console.WriteLine("Op welke poort ben ik server? ");
            args = arg;

            if (arg.Length > 0)
            {
                MijnPoort = int.Parse(args[0]);
                args = arg;
            }
            else
            {
                MijnPoort = int.Parse(Console.ReadLine());
              
            }
            new Server(MijnPoort);
            Console.WriteLine("ik ben server: " + MijnPoort);
            Console.Title = "NetChange " + MijnPoort.ToString();
            
            //args = Console.ReadLine().Split(' ');
            //threads[0] = new Thread(RecieveMessage);
            threads[1] = new Thread(ConsoleInteract);
            threads[2] = new Thread(GetRoutingTable);
            threads[3] = new Thread(AcceptConnection);
            threads[1].Start(1);
            threads[2].Start(2);
           // threads[0].Start(0);
            threads[3].Start(3);

            //threads[1].Join();
            threads[2].Join();
            //threads[3].Join();
            //threads[0].Join();
            RoutingTable.Add(MijnPoort, 0);

        }
  
        static public void GetRoutingTable(object mt)
        {
            
                for (int i = 0; i < Buren.Count; i++)
                {
                    lock (Lock)
                    {
                        if (!RoutingTable.Keys.Contains(Buren.Keys.ElementAt(i)))
                        {
                            RoutingTable.Add(Buren.Keys.ElementAt(i), 1);

                        }
                    }
                }
            
        }

        static public void RecieveMessage(string line)
        {

            string[] poorten = line.Split(' ');

            if (line.StartsWith("Neighbours"))
            {
                Console.WriteLine("Neighbours bericht ontvangen");
                int poort = int.Parse(poorten[1]);
                int cost = int.Parse(poorten[2]);
                if (RoutingTable.ContainsKey(poort))
                {
                    if (cost < RoutingTable[poort])
                    {
                        lock (Lock)
                        {
                            RoutingTable.Remove(poort);
                            RoutingTable.Add(poort, cost);
                        }
                    }
                }
                if (!Buren.ContainsKey(poort) || !RoutingTable.ContainsKey(poort))
                {
                    lock (Lock)
                    {
                        RoutingTable.Add(poort, cost + 1);
                        Console.WriteLine("waarde toegevoegd aan mijn eigen ({0}) routingtable : {1}", MijnPoort, poort);
                    }
                }

            }

            if (line.StartsWith("getNeighbours"))
            {
                Console.WriteLine("getNeighbours bericht ontvangen op poort {0}", MijnPoort);
                int poort = int.Parse(poorten[1]);
                Console.WriteLine("getNeighbours bericht ontvangen vanaf poort {0}", poort);
                for (int i = 0; i < Buren.Count; i++)
                {
                    Buren[poort].Write.WriteLine("Neighbours: {0} {1}", RoutingTable.Keys.ElementAt(i), RoutingTable.Values.ElementAt(i));
                }
            }

        }

        static public void ConsoleInteract(object mt)
        {
            b = false;
            while (true)
            {
                string line = Console.ReadLine();
                string[] input = line.Split(' ');
                if (input[0] == "R")
                {
                    Console.WriteLine("input R wordt herkend");
                    for (int i = 0; i < Buren.Count; i++)
                    {
                        Console.WriteLine(Buren.ElementAt(i));
                    }

                }
                if (input[0] == "B")
                {
                    Console.WriteLine("input B wordt herkend");

                    if (Buren.ContainsKey(int.Parse(input[1])))
                    {
                        string msg = input[2];
                        for (int i = 3; i < input.Length; i++)
                        {
                            msg += (" " + input[i]);
                        }
                        Buren[int.Parse(input[1])].Write.WriteLine(msg);
                    }
                    else
                        Console.WriteLine("Poort niet gevonden");
                }
                if (input[0] == "C")
                {
                    lock (Lock)
                    {
                        int poort = int.Parse(input[1]);
                        if (Buren.ContainsKey(poort))
                            Console.WriteLine("Hier is al verbinding naar!");
                        else
                        {
                            // Leg verbinding aan (als client)
                            Buren.Add(poort, new Connection(poort));
                            RoutingTable.Add(poort, 0);

                        }
                    }
                }
                if (input[0] == "D")
                {
                    lock(Lock)
                    {
                        int poort = int.Parse(input[1]);
                        if (!Buren.ContainsKey(poort))
                            Console.WriteLine("Hier is geen verbinding naar");
                        else
                        {
                            Buren.Remove(poort);
                            RoutingTable.Remove(poort);
                        }
                    }
                }
                if (input[0] == "T")
                {
                    Buren[int.Parse(input[1])].Write.WriteLine("getNeighbours: {0}", MijnPoort);
                    Console.WriteLine("input T wordt herkend");
                }
                if (input[0] == "RT")
                {
                    threads[2] = new Thread(GetRoutingTable);
                    threads[2].Start();
                    for (int i = 0; i < RoutingTable.Count; i++)
                    {
                        Console.WriteLine("{0} {1} {2}", RoutingTable.Keys.ElementAt(i), RoutingTable.Values.ElementAt(i), MijnPoort);
                    }
                    threads[2].Join();
                }

                if (line.StartsWith("verbind"))
                {
                    int poort = int.Parse(line.Split()[1]);
                    if (Buren.ContainsKey(poort))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                    {
                        // Leg verbinding aan (als client)
                        Buren.Add(poort, new Connection(poort));
                    }
                }
            }

        }


        static public void AcceptConnection(object mt)
        {

            for (int i = 1; i < args.Length; i++)
            {
                string input = args[i];
                if (int.Parse(args[i]) > 0)
                {
                    lock (Lock)
                    {
                        int poort = int.Parse(input);
                        if (Buren.ContainsKey(poort))
                            Console.WriteLine("Hier is al verbinding naar!");
                        else
                        {
                            // Leg verbinding aan (als client)
                            Buren.Add(poort, new Connection(poort));
                            
                        }
                    }

                }

            }

            /*while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("verbind"))
                {
                    int poort = int.Parse(input.Split()[1]);
                    if (Buren.ContainsKey(poort))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                    {
                        // Leg verbinding aan (als client)
                        Buren.Add(poort, new Connection(poort));
                    }
                }
                else
                {
                    // Stuur berichtje
                    string[] delen = input.Split(new char[] { ' ' }, 2);
                    int poort = int.Parse(delen[0]);
                    if (!Buren.ContainsKey(poort))
                        Console.WriteLine("Hier is geen verbinding naar!");
                    else
                        Buren[poort].Write.WriteLine(MijnPoort + ": " + delen[1]);
                }
            }
            */
            
        }
    }
}
