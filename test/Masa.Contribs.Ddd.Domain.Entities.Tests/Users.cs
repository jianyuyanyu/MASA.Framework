namespace Masa.Contribs.Ddd.Domain.Entities.Tests;

public class Users : AggregateRoot<Guid>
{
    public string Name { get; set; }
}
