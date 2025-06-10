namespace CarInsuranceTgBot.Abstractions;

public interface IUserSessionStore
{
    UserSession GetOrCreateSession(long userId);
}
