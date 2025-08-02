using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using Haondt.Core.Models;

namespace Hestia.Tests.Attributes
{
    public class HestiaAutoDataAttribute() : AutoDataAttribute(() =>
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new OptionalSpecimenBuilder());
        //fixture.Customize<CreateClientRequest>(c => c
        //    .With(x => x.RepresentativeCompanyId, (int?)null));
        return fixture;
    });

    public class OptionalSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is not Type t)
                return new NoSpecimen();

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Optional<>))
            {
                return Activator.CreateInstance(t)!;
            }

            return new NoSpecimen();
        }
    }
}
