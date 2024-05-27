using QWindowToHttp.HttpService;

ProcessHttp.Message += (ss) => PrintMessage(ss);
ProcessHttp.Error += (ss) => PrintError(ss);

var p = new ProcessHttp();


PrintMessage("运行开始");
while (true)
{
    var writer = Console.ReadLine();
    if (writer == "Exit")
    {
        break;
    }
    else
    {
        switch (writer.ToLower())
        {
            case"rfhtml":p.ReloadHtmlFile();break;
        }
    }
}
PrintMessage("运行结束");




void PrintMessage(string msg)
{
    Console.WriteLine(msg);
}
void PrintError(string msg)
{
    var temp = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.ForegroundColor = temp;
    Console.WriteLine(msg);
}