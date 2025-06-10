using CarInsuranceTgBot.Abstractions;

namespace CarInsuranceTgBot.Commands;

public class CommandFactory : ICommandFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _commandMap;

    public CommandFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _commandMap = new Dictionary<string, Type>
        {
            [CommandNames.StartCommand] = typeof(StartCommand),
            [CommandNames.PhotoHandlerCommand] = typeof(PhotoHandlerCommand),
        };
    }

    public BaseCommand? GetCommand(string commandName)
    {
        return _commandMap.TryGetValue(commandName, out var type) ? _serviceProvider.GetService(type) as BaseCommand : null;
    }
}
