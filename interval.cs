using System;
using System.Collections.Generic;

namespace cap
{
	class interval
	{



		public readonly List<interval> children;
		public interval parent;
		public readonly int left;
		public readonly int right;
		public readonly int length;



		public interval(int length, int left, interval interval)
		{
			this.length = length;
			this.left = left;
			right = -1;
			children = new List<interval>();
			if (interval != null)
			{
				children.Add(interval);
			}
		}



		public interval(interval interval, int right)
		{
			length = interval.length;
			left = interval.left;
			this.right = right;

			children = interval.children;
			foreach (interval cinterval in children)
			{
				cinterval.parent = this;
			}
		}



		public void add(interval interval)
		{
			interval.parent = this;
			children.Add(interval);
		}



		public IEnumerable<int> indexes()
		{
			int l = left;
			int ci = 0;

			while (ci < children.Count)
			{
				while (l < children[ci].left)
				{
					yield return l++;
				}
				l = children[ci].right + 1;
				ci++;
			}

			while (l <= right)
			{
				yield return l++;
			}

			yield break;
		}



	}
}
