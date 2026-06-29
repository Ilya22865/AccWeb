namespace Shared.Events;

public record UserRegisteredEvent(
    int AuthUserId,
    string FullName,
    string Email
);