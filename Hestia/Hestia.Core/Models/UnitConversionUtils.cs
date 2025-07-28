using Hestia.Core.Constants;

namespace Hestia.Core.Models
{
    public static class UnitConversionUtils
    {
        /// <summary>
        /// Snaps a decimal value to the nearest representable value within a specified relative tolerance, given as a percentage of the value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="relativeTolerance">0-1</param>
        /// <returns></returns>
        public static decimal Snap(decimal value, decimal relativeTolerance = HestiaConstants.UnitConversionRelativeTolerance)
        {
            if (value == 0) return 0;

            var sign = Math.Sign(value);
            value = Math.Abs(value);

            var exponent = (int)Math.Floor(Math.Log10((double)value));
            var scale = (decimal)Math.Pow(10, exponent);
            var normalized = value / scale;

            int maxDecimals = MaxDecimalsForTolerance(relativeTolerance);

            for (int d = 0; d <= maxDecimals; d++)
            {
                var roundedNorm = Math.Round(normalized, d, MidpointRounding.AwayFromZero);
                var snapped = roundedNorm * scale;

                var diff = Math.Abs(snapped - value);
                var percentDiff = diff / value;

                if (percentDiff <= relativeTolerance)
                    return snapped * sign;
            }

            return value * sign;
        }

        /// <summary>
        /// Calculates the maximum number of decimal places that can be used for a given tolerance.
        /// </summary>
        /// <param name="tolerance">0-1</param>
        /// <returns></returns>
        private static int MaxDecimalsForTolerance(decimal tolerance)
        {
            if (tolerance <= 0) return 0;
            double t = (double)tolerance;
            int decimals = (int)Math.Ceiling(-Math.Log10(2 * t));
            return decimals < 0 ? 0 : decimals;
        }


    }
}
