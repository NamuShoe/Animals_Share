// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("5zN62s1ywMzjnLjBqSQYut9Ud0i1n0IIPr2lUlwV77583c/yMUPHtroIi6i6h4yDoAzCDH2Hi4uLj4qJXom85LFQ3z6LpPUvp+rcjwaqnoLVbe9XPxmXt0MXt2bQznBdsCRQjmgxRZy3kLrKPaWjn14kbBHgvC0laO4Vw894BJgthKRkRkAUYUJUcNVLJG42yCMVhbjjrx8LKbbmMwpo1BC5B+J4pXFrPvdKRpAJay7IyEFP07XYJKkRuxKieOcx0HuNJUYnudgaNYjKN2iVOcv1iYBiTd3ZX7vuKwiLhYq6CIuAiAiLi4pbIPscoAnzb6YZA9Rm/0YGbvsI7DR7v2U1OavFk1rJyVvc7Y+N7u7KwT2nIr246ZKuEJvt4lcacYiJi4qL");
        private static int[] order = new int[] { 4,6,4,4,5,12,12,9,8,10,13,12,13,13,14 };
        private static int key = 138;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
