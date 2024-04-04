namespace ValleyVisionSolution.Pages.DataClasses
{
    public static class Extension
    {
        public static double[] Populate(this double[] array, double value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
            return array;
        }
    }
}
