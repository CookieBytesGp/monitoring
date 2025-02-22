

using Application.Interfaces;

namespace Domain.SeedWork
{
	public abstract class AggregateRoot : Entity, IAggregateRoot
	{
		protected AggregateRoot() : base()
		{
			_domainEvents =
				new System.Collections.Generic.List<IDomainEvent>();
        }

		// **********
		[System.Text.Json.Serialization.JsonIgnore]
		private readonly System.Collections.Generic.List<IDomainEvent> _domainEvents;

        [System.Text.Json.Serialization.JsonIgnore]
		public System.Collections.Generic.IReadOnlyList<IDomainEvent> DomainEvents
		{
			get
			{
				return _domainEvents;
			}
		}
		// **********

		protected void RaiseDomainEvent(IDomainEvent domainEvent)
		{
			if (domainEvent is null)
			{
				return;
			}

			// **************************************************
			_domainEvents?.Add(domainEvent);
			// **************************************************

			// **************************************************
			//if (_domainEvents is not null)
			//{
			//	_domainEvents.Add(domainEvent);
			//}
			// **************************************************
		}

		protected void RemoveDomainEvent(IDomainEvent domainEvent)
		{
			if (domainEvent is null)
			{
				return;
			}

			// **************************************************
			_domainEvents?.Remove(domainEvent);
			// **************************************************

			// **************************************************
			//if (_domainEvents is not null)
			//{
			//	_domainEvents.Add(domainEvent);
			//}
			// **************************************************
		}

		public void ClearDomainEvents()
		{
			// **************************************************
			_domainEvents?.Clear();
			// **************************************************

			// **************************************************
			//if (_domainEvents is not null)
			//{
			//	_domainEvents.Clear();
			//}
			// **************************************************
		}
	}
}
