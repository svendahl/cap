using System;
using System.Collections.Generic;

namespace cap
{
	class pqueue<Tk, Tv> where Tk : IComparable
	{



		private List<Tk> _k;
		private List<Tv> _v;



		public pqueue(Tk k, Tv v)
		{
			_k = new List<Tk>() { k };
			_v = new List<Tv>() { v };
		}



		public int count
		{
			get { return _k.Count - 1; }
		}



		public Tuple<Tk,Tv> dequeue()
		{
			Tuple<Tk,Tv> output;
			if (_v.Count > 1)
			{
				output = new Tuple<Tk, Tv>(_k[1], _v[1]);

				_k[1] = _k[_k.Count - 1];
				_v[1] = _v[_v.Count - 1];
				_k.RemoveAt(_k.Count - 1);
				_v.RemoveAt(_v.Count - 1);

				leveldown(1);
			}
			else
			{
				throw new Exception("Queue empty");
			}

			return output;
		}



		public bool empty
		{
			get { return _k.Count == 1; }
		}



		public void enqueue(Tk k, Tv v)
		{
			_k.Add(k);
			_v.Add(v);
			levelup(_v.Count - 1);
		}



		private int leftchildof(int index)
		{
			return index <<= 1;
		}



		private int rightchildof(int index)
		{
			return leftchildof(index) + 1;
		}



		private int parentof(int index)
		{
			return index >>= 1;
		}



		private void leveldown(int index)
		{
			int parent = index;

			while (true)
			{
				int childindex = leftchildof(parent);
				if (childindex >= _k.Count)
				{
					break;
				}

				int rightchild = rightchildof(parent);
				if (rightchild < _k.Count && _k[rightchild].CompareTo(_k[childindex]) < 0)
				{
					childindex = rightchild;
				}

				if (_k[parent].CompareTo(_k[childindex]) < 0)
				{
					break;
				}
				swap(childindex, parent);
				parent = childindex;
			}
		}



		private void levelup(int index)
		{
			int parent = parentof(index);

			while (parent >= 1)
			{
				if (_k[index].CompareTo(_k[parent]) > 0)
				{
					break;
				}

				swap(index, parent);

				index = parent;
				parent = parentof(index);
			}
		}



		public bool isvalid
		{
			get
			{
				for (int i = 1; i < _k.Count; i++)
				{
					int parent = i;
					int leftchild = leftchildof(parent);

					if (leftchild < _k.Count && _k[leftchild].CompareTo(_k[parent]) < 0)
					{
						return false;
					}

					int rightchild = rightchildof(parent);
					if (rightchild < _k.Count && _k[rightchild].CompareTo(_k[parent]) < 0)
					{
						return false;
					}
				}
				return true;
			}
		}



		private void swap(int i1, int i2)
		{
			var tk = _k[i1];
			var tv = _v[i1];

			_k[i1] = _k[i2];
			_v[i1] = _v[i2];
			_k[i2] = tk;
			_v[i2] = tv;
		}



	} // class
} // namespace
