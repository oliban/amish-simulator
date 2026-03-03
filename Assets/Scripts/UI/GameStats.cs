using System;

namespace AmishSimulator
{
    [Serializable]
    public class GameStats
    {
        public int Age;
        public int DaysLived;
        public float ButterChurned;       // pounds
        public int ChildrenCount;
        public int AcresPlowed;
        public float AverageAffinity;
        public float BeardLengthInches;   // male only
        public int YearsServed;

        public void AddButter(float amount) => ButterChurned += amount;
        public void AddAcre() => AcresPlowed++;
        public void IncrementChildren() => ChildrenCount++;

        public int CalculateScore()
        {
            // Weighted score formula
            return (int)(
                AcresPlowed      * 100 +
                ChildrenCount    * 500 +
                AverageAffinity  * 10  +
                ButterChurned    * 2   +
                BeardLengthInches * 50 +
                YearsServed      * 200
            );
        }
    }
}
