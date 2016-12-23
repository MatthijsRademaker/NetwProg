﻿using System;
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

        static void Main(string[] arg)
        {
            Console.WriteLine("Op welke poort ben ik server? ");
            args = arg;

            if (args.Length > 0)
            {
                MijnPoort = int.Parse(args[0]);
            }
            else
            {
                MijnPoort = int.Parse(Console.ReadLine());
            }

            Console.WriteLine("ik ben server: " + MijnPoort);
            new Server(MijnPoort);
            Console.Title = "NetChange " + MijnPoort.ToString();
            Thread[] threads = new Thread[4];
            
            //threads[0] = new Thread(RecieveMessage);
            threads[1] = new Thread(ConsoleInteract);
            threads[2] = new Thread(GetRoutingTable);
            threads[3] = new Thread(AcceptConnection);
            threads[1].Start(1);
            threads[2].Start(2);
            //threads[0].Start(0);
            threads[3].Start(3);
            
            threads[1].Join();
            threads[2].Join();
            threads[3].Join();
            //threads[0].Join();


        }
  
        static public void GetRoutingTable(object mt)
        {
            while (b)
            {
                Console.WriteLine("getRouting table werkt wel vanuit poort: {0} thread: {1} ", MijnPoort, mt);
                for (int i = 0; i < Buren.Count; i++)
                {
                    List<int> b = Buren.Keys.ToList();
                    Buren[b[i]].Write.WriteLine("getNeighbours: {0}", MijnPoort);
                    Console.WriteLine("de for loop in get routing table werkt wel");
                }
            }
        }

        static public void RecieveMessage(string line)
        {
            
            string[] poorten = line.Split(' ');
                
            if (line.StartsWith("Neigbours"))
            {
                for (int i = 1; i < poorten.Length; i++)
                {
                    int poort = int.Parse(poorten[i]);
                    if (!Buren.ContainsKey(poort))
                    {
                        RoutingTable.Add(poort, 1);
                    }
                }
            }
            if (line.StartsWith("getNeighbours"))
            {
                int poort = int.Parse(poorten[1]);
                for (int i = 0; i < Buren.Count; i++)
                {
                    Buren[poort].Write.WriteLine("Neighbours: {0}", Buren.Keys.ElementAt(i));
                }
            }
            
        }

        static public void ConsoleInteract(object mt)
        {
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

                }
                if (input[0] == "C")
                {

                }
                if (input[0] == "D")
                {

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
                    int poort = int.Parse(input);
                    lock (Buren)
                    {

                        if (Buren.ContainsKey(poort))
                            Console.WriteLine("Hier is al verbinding naar!");
                        else
                        {
                            // Leg verbinding aan (als client)
                            if ((a = new Connection(poort)) != null)
                            {
                                Buren.Add(poort, a);
                                RoutingTable.Add(poort, 0);
                                Buren[poort].Write.WriteLine("getNeighbours: {0}", MijnPoort);
                                //Console.WriteLine("ik verstuur wel berichten in de routingtable acceptconnection method");
                            }
                            Console.WriteLine("er is nu verbinding met poort als client: " + poort);
                            b = true;
                        }
                    }
                }
                else
                {
                    // Stuur berichtje
                    string[] delen = input.Split(new char[] { ' ' }, 2);
                    int poort = int.Parse(delen[0]);
                    if (!Buren.ContainsKey(poort))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                        Buren[poort].Write.WriteLine(MijnPoort + ": " + delen[1]);
                }
            }

            
        }
    }
}
