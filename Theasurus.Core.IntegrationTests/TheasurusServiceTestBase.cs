using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Theasurus.Core.Test
{
	[TestFixture]
	public class TheasurusServiceTestBase
	{
		protected DbContextOptions<TheasurusDbContext> ContextOptions { get; private set; }

		[SetUp]
		public virtual void Setup()
		{
			ContextOptions = new DbContextOptionsBuilder<TheasurusDbContext>().UseSqlite("Filename=Test.db").Options;

			using var context = new TheasurusDbContext(ContextOptions);

			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();

			var anger = new Word("anger");
			var rage = new Word("rage");
			var irritation = new Word("irritation");

			var angerAdded = context.Add(anger);
			var rageAdded = context.Add(rage);
			var irritationAdded = context.Add(irritation);

			context.SaveChanges();

			var synonym1 = new WordSynonym(angerAdded.Entity.Id, rageAdded.Entity.Id);
			var synonym2 = new WordSynonym(angerAdded.Entity.Id, irritationAdded.Entity.Id);

			context.AddRange(synonym1, synonym2);

			context.SaveChanges();
		}
	}
}