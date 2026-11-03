using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace Household.Api.Tests.Features.Account.Support;

// Isolates both authentication and Data Protection's startup key-ring warmup.
// Keys exist only for the lifetime of one test host; no developer key files are read or written.
internal sealed class TestKeyRepository : IXmlRepository
{
    private readonly List<XElement> _elements = [];

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        lock (_elements)
            return _elements.Select(element => new XElement(element)).ToArray();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        lock (_elements)
            _elements.Add(new XElement(element));
    }
}
