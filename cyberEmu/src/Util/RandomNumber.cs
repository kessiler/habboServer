using System;
namespace Cyber.Util
{
	internal static class RandomNumber
	{
		private static Random r = new Random();
		private static object l = new object();
		private static Random globalRandom = new Random();
		[ThreadStatic]
		private static Random localRandom;
		public static int GenerateNewRandom(int min, int max)
		{
            return new Random((int)DateTime.Now.Ticks).Next(min, max);
		}
		public static int GenerateLockedRandom(int min, int max)
		{
			int result;
			lock (RandomNumber.l)
			{
				result = RandomNumber.r.Next(min, max);
			}
			return result;
		}
		public static int GenerateRandom(int min, int max)
		{
			Random random = RandomNumber.localRandom;
			if (random == null)
			{
				int seed;
				lock (RandomNumber.globalRandom)
				{
					seed = RandomNumber.globalRandom.Next();
				}
				random = (RandomNumber.localRandom = new Random(seed));
			}
			return random.Next(min, max);
		}
	}
}
