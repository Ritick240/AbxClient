using System;
using AbxClient.Repo;

class Program
{
    static void Main()
    {
        Console.WriteLine("Starting..");
        AbxRepo repo = new AbxRepo();
        repo.ServerConnection();
        Console.WriteLine("Finish...");
    }

   
}
