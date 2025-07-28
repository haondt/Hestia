using Hestia.Persistence.Models;

namespace Hestia.Domain.Models
{
    public record UnitConversionModel
    {
        public required string FromUnit { get; set; }
        public required string ToUnit { get; set; }
        public required decimal Multiplier { get; set; }

        public static UnitConversionModel FromDataModel(UnitConversionDataModel dataModel)
        {
            return new UnitConversionModel
            {
                FromUnit = dataModel.FromUnit,
                ToUnit = dataModel.ToUnit,
                Multiplier = dataModel.Multiplier
            };
        }


        public UnitConversionDataModel AsDataModel() => new()
        {
            FromUnit = FromUnit,
            ToUnit = ToUnit,
            Multiplier = Multiplier,
            NormalizedFromUnit = FromUnit,
            NormalizedToUnit = ToUnit
        };
    }
}

