using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Monitoring.Domain.SeedWork;

	public abstract class ValueObject : object
	{
		#region Static Member(s)
		public static bool operator ==(ValueObject leftObject, ValueObject rightObject)
		{
			if (leftObject is null && rightObject is null)
			{
				return true;
			}

			if (leftObject is null && rightObject is not null)
			{
				return false;
			}

			if (leftObject is not null && rightObject is null)
			{
				return false;
			}

			return leftObject.Equals(rightObject);
		}

		public static bool operator !=(ValueObject leftObject, ValueObject rightObject)
		{
			return !(leftObject == rightObject);
		}
		#endregion /Static Member(s)

		protected ValueObject() : base()
		{
		}

		protected abstract System.Collections.Generic.IEnumerable<object> GetEqualityComponents();

		public override bool Equals(object anotherObject)
		{
			if (anotherObject is null)
			{
				return false;
			}

			if (GetType() != anotherObject.GetType())
			{
				return false;
			}

			var stronglyTypedOtherObject =
				anotherObject as ValueObject;

			if (stronglyTypedOtherObject is null)
			{
				return false;
			}

			bool result =
				GetEqualityComponents()
				.SequenceEqual(stronglyTypedOtherObject.GetEqualityComponents());

			return result;
		}

		public override int GetHashCode()
		{
			using (var sha256 = SHA256.Create())
			{
				// تبدیل EqualityComponents به رشته برای تولید هش
				var components = GetEqualityComponents()
					.Select(x => x != null ? x.ToString() : string.Empty);

				var concatenatedString = string.Join("|", components); // ترکیب همه مقادیر به یک رشته

				var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString));

				// تبدیل اولین 4 بایت هش به عدد برای GetHashCode
				return BitConverter.ToInt32(hashBytes, 0);
			}
		}
	}
