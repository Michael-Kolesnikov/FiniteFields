﻿using System.Reflection.PortableExecutable;
using System.Xml.Linq;

namespace FF
{
    public class FiniteFieldElement
    {
        public int[] Poly { get; set; }
        public readonly FiniteField field;
        public int element;
        public FiniteFieldElement(int element,FiniteField field)
        {
            Poly = Array.Empty<int>();
            this.field = field;
            this.element = element;
        }
        public FiniteFieldElement(int[] Poly,FiniteField field)
        {
            this.field = field;
            this.Poly = Poly;
            for(var i =  Poly.Length-1; i >= 0; i--)
                element += Poly[i] * (int)Math.Pow(field.characteristic, i);
        }
        public static FiniteFieldElement operator +(FiniteFieldElement el1, FiniteFieldElement el2)
        {
            // return if elements from different fields.
            if (!el1.field.Equals(el2.field))
                throw new InvalidOperationException();
            
            var field = el1.field;
            var maxDegreeElement = el1.Poly.Length > el2.Poly.Length ? el1 : el2;
            var minDegreeElement = el1.Equals(maxDegreeElement) ? el2 : el1;

            var sum = maxDegreeElement;

            var index = 0;
            for (;index < minDegreeElement.Poly.Length; index++)
                sum.Poly[index] = mod(maxDegreeElement.Poly[index] + minDegreeElement.Poly[index], field.characteristic);
            return sum;
        }
        public static FiniteFieldElement operator -(FiniteFieldElement el1, FiniteFieldElement el2)
        {
            if (!el1.field.Equals(el2.field))
                throw new InvalidOperationException();
            var field = el1.field;
            if (field.isPrimeField)
                return SubstractionPrimeFieldElements(el1, el2);
            else
                return SubstractionNoPrimeFieldElements(el1, el2);
            
        }
        private static FiniteFieldElement SubstractionPrimeFieldElements(FiniteFieldElement el1, FiniteFieldElement el2)
        {
            el1.element = mod(el1.element - el2.element,el1.field.characteristic);
            return el1;
        }
        private static FiniteFieldElement SubstractionNoPrimeFieldElements(FiniteFieldElement el1, FiniteFieldElement el2)
        {
            var field = el1.field;
            var substract = el1;
            if (el1.Poly.Length < el2.Poly.Length)
            {
                var difference = el2.Poly.Length - el1.Poly.Length;
                var el1PolyExtended = new int[el2.Poly.Length];
                for (var i = 0; i < difference; i++)
                    el1PolyExtended[i] = 0;
                for (var i = difference; i < el2.Poly.Length; i++)
                    el1PolyExtended[i] = el1.Poly[i];
                el1.Poly = el1PolyExtended;
            }
            for (var i = 0; i < el2.Poly.Length; i++)
                substract.Poly[i] = mod(el1.Poly[i] - el2.Poly[i], field.characteristic);
            return substract;
        }

        public static FiniteFieldElement operator /(FiniteFieldElement el1, FiniteFieldElement el2)
        {
            if (!el1.field.Equals(el2.field))
                throw new InvalidOperationException();
            return null;
        }
        public static FiniteFieldElement operator *(FiniteFieldElement el1, FiniteFieldElement el2)
        {
            if (!el1.field.Equals(el2.field))
                throw new InvalidOperationException();
            var field = el1.field;
            var multPoly = new int[el1.Poly.Length + el2.Poly.Length - 1];
            for(var i = el1.Poly.Length - 1; i >=0 ;i--)
            {
                for(var j = el2.Poly.Length -1;j >= 0;j--)
                {
                    multPoly[i + j] += el1.Poly[i] * el2.Poly[j];
                }
            }

            return new FiniteFieldElement(multPoly, field);
        }
        public static FiniteFieldElement operator %(FiniteFieldElement el1, FiniteFieldElement el2)
        {
            if (!el1.field.Equals(el2.field))
                throw new InvalidOperationException();

            var field = el1.field;
            var dividend = el1.Poly;
            var divisor = el2.Poly;

            int dividendDegree = dividend.Length - 1;
            int divisorDegree = divisor.Length - 1;

            if (dividendDegree < divisorDegree)
            {
                return el1;
            }

            int[] quotient = new int[dividendDegree - divisorDegree + 1];
            int[] remainder = new int[dividendDegree + 1];
            Array.Copy(dividend, remainder, dividendDegree + 1);

            for (int i = dividendDegree - divisorDegree; i >= 0; i--)
            {
                quotient[i] = remainder[i + divisorDegree] / divisor[divisorDegree];

                for (int j = i + divisorDegree; j >= i; j--)
                {
                    remainder[j] -= quotient[i] * divisor[j - i];
                }
            }

            Array.Resize(ref remainder, divisorDegree);
            return new FiniteFieldElement(remainder, field);
        }
        public override bool Equals(object? obj)
        {
            if (obj is not FiniteFieldElement el) return false;
            if (el.Poly.Length != this.Poly.Length) return false;
            if (!el.field.Equals(this.field)) return false;
            for(var i = 0; i < this.Poly.Length;i++)
                if (el.Poly[i] != this.Poly[i]) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return field.GetHashCode() + Poly.GetHashCode();
        }
        private static int mod(int k, int n) => ((k %= n) < 0) ? k + n : k;

    }
}
