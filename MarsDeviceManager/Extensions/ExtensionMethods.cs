using MarsDeviceManager.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MarsDeviceManager.Extensions
{
	/// <summary>
	/// Contains extension methods used in the <see cref="MarsDeviceManager"/> library
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Update every value in an object with new values from another object
		/// as long as the value is not null
		/// </summary>
		/// <typeparam name="T">objects type</typeparam>
		/// <param name="oldObj">object to be updated</param>
		/// <param name="newObj">object to update the values from</param>
		/// <returns>An object with updated values</returns>
		public static T UpdateValues<T>(this T oldObj, T newObj)
		{
			if (newObj == null)
			{
				// do nothing
				return oldObj;
			}
			if (oldObj == null)
			{
				// return the new object
				return newObj;
			}

			Type oldType = oldObj.GetType();
			Type newType = newObj.GetType();

			// if the real type is different
			// i.e. called as objects but runtime types are different
			if (newType != oldType)
			{
				// do nothing
				Console.WriteLine("Runtime types are not the same, operation cancelled");
				return oldObj;
			}

			// if it is a primitive value
			if (oldType.IsPrimitive || oldType == typeof(string) || oldType.IsEnum)
			{
				// simply insert the new value
				return newObj;
			}

			// iterate over the old object properties
			foreach (PropertyInfo property in oldType.GetProperties().Where(x=>x.CanWrite))
			{
				#region Array handeling
				// if the property is an array
				if (property.PropertyType.IsArray)
				{
					var oldList = ((Array)property.GetValue(oldObj)).OfType<object>().ToList();
					var newList = ((Array)property.GetValue(newObj))?.OfType<object>().ToList();

					// if there are any items in the new object's list
                    if (newList != null)
                    {
	                    // iterate over the new array and update/add values
                        foreach (var newItem in newList)
                        {
                            // if old array has this value: update old value
                            try
                            {
                                var oldItemIdx
                                    = oldList.FindIndex(x => IsIdenticalType(x, newItem));
                                if (oldItemIdx != -1)
                                {
                                    // update old item
                                    oldList[oldItemIdx] = oldList[oldItemIdx].UpdateValues(newItem);
                                }
                                else
                                {
                                    // add new item to the old array
                                    oldList.Add(newItem);
                                }
                            }
                            catch (ArgumentNullException)
                            {
                                // if just one parameter is null, ignore
                            }
                        }
                    }
					// set the updated array to the old object
					//var updatedValue = oldList.Select(x => Convert.ChangeType(x, property.PropertyType.GetElementType()));
					//var updatedValue = oldList.ConvertAll(x=> x);
					//TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
					//var updatedValue = converter.ConvertFrom(oldList);
					//property.SetValue(oldObj, updatedValue);
				}
				else
				{
					// take new value
					object newValue = property.GetValue(newObj);

					// ignore null values
					if (newValue == null)
					{
						continue;
					}

					// take old value
					object oldValue = property.GetValue(oldObj);

					// update the old value with the new value
					// (over and over until it is a primitive value)
					UpdateValues(oldValue, newValue);
					property.SetValue(oldObj, oldValue);
				}
				#endregion

				// take new value
				//object newValue = property.GetValue(newObj);

				//// ignore null values
				//if (newValue == null)
				//{
				//	continue;
				//}

				//// take old value
				//object oldValue = property.GetValue(oldObj);

				//// update the old value with the new value
				//// (over and over until it is a primitive value)
				//property.SetValue(oldObj, UpdateValues(oldValue, newValue));
			}

			// after all the values are updated, return the (updated) old object
			return oldObj;
		}

		private static bool IsIdenticalType(object a, object b)
		{
			// handle null values
			if (a == null && b == null)
			{
				return true;
			}
			else if (a == null || b == null)
			{
				throw new ArgumentNullException($"{a ?? b}", "one of the parameters was null");
			}

			Type aType = a.GetType();
			Type bType = b.GetType();

			bool result = true;
			if (aType.IsPrimitive || aType == typeof(string) || aType.IsEnum || !aType.IsValueType)
			{
				// simply return the new value
				result = aType == bType;
			}
			else if (aType.IsArray && bType.IsArray)
			{
				Array aArr = (Array)a;
				Array bArr = (Array)b;

				return aArr.GetType() == bArr.GetType();
			}
			else if (aType == bType)
			{
				foreach (var property in aType.GetProperties())
				{
					var aValue = property.GetValue(a);
					var bValue = property.GetValue(b);

					result &= IsIdenticalType(aValue, bValue);
				}
			}
			else
			{
				result = aType == bType;
			}

			return result;
		}
	}
}
