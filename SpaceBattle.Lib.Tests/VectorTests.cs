using System;

namespace SpaceBattle.Lib.Test
{
    public class VectorTests
    {
        [Fact]
        public void NullVector()
        {
            Assert.Throws<ArgumentException>(() => new Vector());
        }

        [Fact]
        public void SumSameSize()
        {
            Vector a = new(0, 1);
            Vector b = new(2, 3);
            Assert.True(a + b == new Vector(2, 4));
        }

        [Fact]
        public void AddDifferentSize()
        {
            Vector a = new(1, 2, 3);
            Vector b = new(3, 4);

            Assert.Throws<ArgumentException>(() => a + b);
        }

        [Fact]
        public void CompareOneSizeVectorsWithDifferentCoords()
        {
            Vector a = new(0, 1);
            Vector b = new(0, 2);

            Assert.False(a == b);
        }

        [Fact]
        public void CompareOneSizeVectorsWithSameCoords()
        {
            Vector a = new(0, 1);
            Vector b = new(0, 1);

            Assert.True(a == b);
        }

        [Fact]
        public void CompareDifferentSizeVectors()
        {
            Vector a = new(0, 1, 2);
            Vector b = new(0, 2);

            Assert.Throws<ArgumentException>(() => a == b);
        }

        [Fact]
        public void NotCompareOneSizeVectorsWithSameCoords()
        {
            Vector a = new(0, 1);
            Vector b = new(0, 1);

            Assert.False(a != b);
        }

        [Fact]
        public void NotCompareOneSizeVectorsWithDifferentCoords()
        {
            Vector a = new(0, 1);
            Vector b = new(0, 2);

            Assert.True(a != b);
        }

        [Fact]
        public void NotCompareDifferentSizeVectors()
        {
            Vector a = new(0, 1, 2);
            Vector b = new(0, 2);

            Assert.Throws<ArgumentException>(() => a != b);
        }

        [Fact]
        public void VGetHashCode()
        {
            Vector a = new(0, 1);
            var code = a.GetHashCode();
            Assert.Equal(code, a.GetHashCode());
        }

        [Fact]
        public void EqualTwoDiffThings()
        {
            Vector a = new(0, 1);
            var b = new int[] { 0, 1 };
            var Res = a.Equals(b);

            Assert.Equal(a.Equals(b), Res);
        }
    }
}