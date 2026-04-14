using System;

namespace QuoteFetcher;

public interface IConsoleWrapper
{
    ConsoleKeyInfo ReadKey();
    string ReadLine();
    void WriteLine(string value = "");
}

internal class ConsoleWrapper : IConsoleWrapper
{
    public ConsoleKeyInfo ReadKey() => Console.ReadKey();
    public string ReadLine() => Console.ReadLine();
    public void WriteLine(string value = "") => Console.WriteLine(value);
}