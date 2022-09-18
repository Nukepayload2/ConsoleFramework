Nukepayload2.ConsoleFramework.Application.Run(
async ValueTask (int port) =>
{
    Console.WriteLine(3);
    await Task.Delay(1000);
    Console.WriteLine(2);
    await Task.Delay(1000);
    Console.WriteLine(1);
    await Task.Delay(1000);
    Console.WriteLine("Started at " + port);
});