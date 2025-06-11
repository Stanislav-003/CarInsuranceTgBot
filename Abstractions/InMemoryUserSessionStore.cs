using System.Collections.Concurrent;

namespace CarInsuranceTgBot.Abstractions;

// Імітація бд, де long - це ID користувача, а UserSession - це допоміжні дані
public class InMemoryUserSessionStore : IUserSessionStore
{
    // зберігаю стан користувачів у багатопотоковому середовищі
    private readonly ConcurrentDictionary<long, UserSession> _sessions = new();

    public UserSession GetOrCreateSession(long userId)
    {
        return _sessions.GetOrAdd(userId, _ => new UserSession());
    }
}
