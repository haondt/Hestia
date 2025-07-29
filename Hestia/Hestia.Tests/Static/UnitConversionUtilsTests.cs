using Hestia.Core.Models;

namespace Hestia.Tests.Static
{
    public class UnitConversionUtilsTests
    {
        [Theory]
        [InlineData(1.99, 0.1, 2.00)] // Basic snapping
        [InlineData(1.9999, 0.01, 2.00)] // Rounding up
        [InlineData(1.0001, 0.01, 1.00)] // Rounding down
        [InlineData(135, 0.1, 140)] // small leading decimal
        [InlineData(535, 0.1, 500)] // large leading decimal
        [InlineData(1.23456789, 0.0001, 1.2346)] // Higher precision
        [InlineData(100.123, 0.000001, 100.123)] // Already precise
        [InlineData(0, 0.01, 0)] // Zero value
        [InlineData(-1.25, 0.1, -1.3)] // Negative value rounding up
        [InlineData(-1.22, 0.1, -1.2)] // Negative value rounding down
        [InlineData(12345.6789, 0.00001, 12345.7)] // Large number, high precision
        [InlineData(525, 0.05, 500)] // non-multiple of ten tolerance
        [InlineData(526, 0.05, 500)] // non-multiple of ten tolerance
        [InlineData(524, 0.05, 500)] // non-multiple of ten tolerance
        // misc cases
        [InlineData(0.100000001, 0.01, 0.1)]
        [InlineData(525, 0.01, 530)]
        [InlineData(524, 0.01, 520)]
        [InlineData(0.0525, 0.01, 0.0530)]
        [InlineData(0.0524, 0.01, 0.0520)]
        [InlineData(21.001, 0.01, 21)]
        [InlineData(43705.05, 0.02, 44000)]
        [InlineData(0.0007600008, 0.02, 0.00076)]

        public void Snap_ShouldReturnExpectedValue(decimal value, decimal relativeTolerance, decimal expected)
        {
            // Act
            var result = UnitConversionUtils.Snap(value, relativeTolerance);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1.2345, 0.000000000000000000000000000001, 1.2345)] // Extremely low tolerance
        [InlineData(1.2345, 0.999999999999999999999999999999, 1)] // Extremely high tolerance
        [InlineData(0.000000000000000000000000000001, 0.01, 0)] // Extremely small positive value
        [InlineData(-0.000000000000000000000000000001, 0.01, 0)] // Extremely small negative value
        [InlineData(1, 0, 1)] // Zero tolerance
        [InlineData(999999999999, 0.01, 1000000000000)] // Very large number
        [InlineData(0.5, 1, 1)] // Tolerance equals 100%
        [InlineData(1.5, 0.5, 2)] // 50% tolerance
        public void Snap_EdgeCases(decimal value, decimal relativeTolerance, decimal expected)
        {
            // Act
            var result = UnitConversionUtils.Snap(value, relativeTolerance);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
