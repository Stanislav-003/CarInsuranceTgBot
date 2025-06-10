namespace CarInsuranceTgBot.Abstractions;

// Імітація бд, де long - це ID користувача, а UserSession - це допоміжні дані
public class InMemoryUserSessionStore : IUserSessionStore
{
    private readonly Dictionary<long, UserSession> _sessions = new();

    public UserSession GetOrCreateSession(long userId)
    {
        // Імітація наче в базу був доданий користувач, або отриманий інсуючий
        if (!_sessions.ContainsKey(userId))
            _sessions[userId] = new UserSession();

        return _sessions[userId];
    }
}
