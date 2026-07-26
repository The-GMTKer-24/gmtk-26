namespace Player
{
    public class PersistentData
    {
        public float CurrentTime;
        public float BestTime = float.PositiveInfinity;
        public static PersistentData Instance
        {
            get
            {
                data ??= new PersistentData();
                return data;
            }
        }
        private static PersistentData data;
        
    }
}