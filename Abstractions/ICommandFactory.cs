using CarInsuranceTgBot.Commands;

namespace CarInsuranceTgBot.Abstractions;

public interface ICommandFactory
{
    BaseCommand? GetCommand(string commandName);
}
