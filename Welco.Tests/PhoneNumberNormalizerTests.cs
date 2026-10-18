using Welco.Shared.Common.Helpers;
using Xunit;

namespace Welco.Tests;

public class PhoneNumberNormalizerTests
{
    [Fact]
    public void Normalize_NullOrEmpty_ReturnsNull()
    {
        Assert.Null(PhoneNumberNormalizer.Normalize(null));
        Assert.Null(PhoneNumberNormalizer.Normalize("   "));
        Assert.Null(PhoneNumberNormalizer.Normalize(""));
    }

    [Fact]
    public void Normalize_WithExplicitPhoneCode_StripsTrunkZero()
    {
        // Pakistan mobile number with trunk 0
        var norm1 = PhoneNumberNormalizer.Normalize("03000959336", "+92");
        Assert.Equal("+923000959336", norm1);

        // Pakistan mobile number without trunk 0
        var norm2 = PhoneNumberNormalizer.Normalize("3000959336", "+92");
        Assert.Equal("+923000959336", norm2);

        // Egypt mobile number with trunk 0
        var norm3 = PhoneNumberNormalizer.Normalize("01022812243", "+20");
        Assert.Equal("+201022812243", norm3);

        // Egypt mobile number without trunk 0
        var norm4 = PhoneNumberNormalizer.Normalize("1022812243", "+20");
        Assert.Equal("+201022812243", norm4);

        // Already has dial code prefixed
        var norm5 = PhoneNumberNormalizer.Normalize("+20 01022812243", "+20");
        Assert.Equal("+201022812243", norm5);
    }

    [Fact]
    public void Normalize_CorruptedPlusOneOnPakistaniNumber_AutoCorrects()
    {
        // Number was mistakenly prepended with +1 on Pakistani 0300...
        var norm = PhoneNumberNormalizer.Normalize("+1 03000959336");
        Assert.Equal("+923000959336", norm);
    }

    [Fact]
    public void Normalize_WithPlusAndTrunkZero_StripsTrunkZero()
    {
        var norm1 = PhoneNumberNormalizer.Normalize("+20 01022812243");
        Assert.Equal("+201022812243", norm1);

        var norm2 = PhoneNumberNormalizer.Normalize("+971 0501234567");
        Assert.Equal("+971501234567", norm2);

        var norm3 = PhoneNumberNormalizer.Normalize("+966 0501234567");
        Assert.Equal("+966501234567", norm3);

        var norm4 = PhoneNumberNormalizer.Normalize("+92 0300 0959336");
        Assert.Equal("+923000959336", norm4);
    }

    [Fact]
    public void AreEquivalent_DifferentFormatsSameNumber_ReturnsTrue()
    {
        Assert.True(PhoneNumberNormalizer.AreEquivalent("+20 01022812243", "+201022812243"));
        Assert.True(PhoneNumberNormalizer.AreEquivalent("00201022812243", "+20 1022812243"));
        Assert.True(PhoneNumberNormalizer.AreEquivalent("+1 03000959336", "+92 300 0959336"));
    }
}