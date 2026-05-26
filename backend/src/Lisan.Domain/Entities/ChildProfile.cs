namespace Lisan.Domain.Entities;

public class ChildProfile
{
    public Guid Id { get; private set; }

    private ChildProfile() { }

    public static ChildProfile Create() => new() { Id = Guid.NewGuid() };
}
