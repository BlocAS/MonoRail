﻿namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Data.Services.Common;
	using System.IO;
	using System.Linq;
	using System.ServiceModel.Syndication;
	using System.Xml;
	using FluentAssertions;
	using NUnit.Framework;

	public partial class SegmentProcessorTestCase
	{
		// naming convention for testing methods
		// [EntitySet|EntityType|PropSingle|PropCollection|Complex|Primitive]_[Operation]_[InputFormat]_[OutputFormat]__[Success|Failure]

		[Test]
		public void EntitySet_ViewWithFilter_Atom_Atom_Success()
		{
			Process("/catalogs/", SegmentOp.View, _model, qs: "$filter=Name eq 'Cat1'");

			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(1);

			Assertion.Callbacks.ViewManyWasCalled(1);
			Assertion.Callbacks.ViewSingleWasCalled(0);

			var item = feed.Items.ElementAt(0);
			var xmlContent = (item.Content as XmlSyndicationContent).GetReaderAtContent();
			xmlContent.ReadStartElement("content", "http://www.w3.org/2005/Atom");
			xmlContent.ReadStartElement("properties", MetadataNs);
			xmlContent.ReadElementString("Id", DataSvsNs);
			var name = xmlContent.ReadElementString("Name", DataSvsNs);

			name.Should().Be("Cat1");

		}

		[Test]
		public void EntitySet_View_Atom_Atom_Success()
		{
			Process("/catalogs/", SegmentOp.View, _model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);

			Assertion.Callbacks.ViewManyWasCalled(1);
			Assertion.Callbacks.ViewSingleWasCalled(0);

			Assertion.Feed(feed, id: "http://localhost/base/catalogs");
			Assertion.FeedLink(feed, title: "Catalog1", rel: "self", href: "catalogs");

			feed.Items.Should().HaveCount(2);
			var entry1 = feed.Items.ElementAt(0);
			Assertion.Entry(entry1, Id: "http://localhost/base/catalogs(1)");
			Assertion.EntryLink(entry1, Title: "Catalog1", Rel: "edit", Href: "catalogs(1)");
			Assertion.EntryLink(entry1, Title: "Product1",
				Rel: "http://schemas.microsoft.com/ado/2007/08/dataservices/related/Product1",
				Href: "catalogs(1)/Products", Media: "application/atom+xml;type=feed");

			var entry2 = feed.Items.ElementAt(1);
			Assertion.Entry(entry2, Id: "http://localhost/base/catalogs(2)");
			Assertion.EntryLink(entry2, Title: "Catalog1", Rel: "edit", Href: "catalogs(2)");
			Assertion.EntryLink(entry2, Title: "Product1",
				Rel: "http://schemas.microsoft.com/ado/2007/08/dataservices/related/Product1",
				Href: "catalogs(2)/Products", Media: "application/atom+xml;type=feed");
		}

		[Test]
		public void EntitySet_WithResourceProperty_View_Atom_Atom_Success()
		{
			Process("/products/", SegmentOp.View, _model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);

			Assertion.Callbacks.ViewManyWasCalled(1);
			Assertion.Callbacks.ViewSingleWasCalled(0);
		}

		[Test]
		public void EntitySet_WithPropMappingToTitle_View_Atom_Atom_Success()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set).
						AddAttribute(new EntityPropertyMappingAttribute("Name", SyndicationItemProperty.Title, SyndicationTextContentKind.Xhtml, false));
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});

			Process("/catalogs/", SegmentOp.View, model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);

			Assertion.Callbacks.ViewManyWasCalled(1);
			Assertion.Callbacks.ViewSingleWasCalled(0);

			// TODO: assert for syndication mapping. <title> should have catalog name
		}

		[Test]
		public void EntitySet_WithComplexType_View_Atom_Atom_Success()
		{
			Process("/suppliers/", SegmentOp.View, _model);

			// Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Items.Should().HaveCount(2);

			Assertion.Callbacks.ViewManyWasCalled(1);
			Assertion.Callbacks.ViewSingleWasCalled(0);
		}

		// todo: tests for all primitive types


		[Test]
		public void EntitySet_View_JSon_Success()
		{
			Process("/catalogs/", SegmentOp.View, _model, accept: MediaTypes.JSon);

			_response.contentType.Should().Be(MediaTypes.JSon);

			_body.ToString().Should().Be(@"{
  ""d"": [
	{
	  ""__metadata"": {
		""uri"": ""http://localhost/base/catalogs(1)"",
		""type"": ""TestNamespace.Catalog1""
	  },
	  ""Id"": 1,
	  ""Name"": ""Cat1"",
	  ""Products"": {
		""__deferred"": {
		  ""uri"": ""http://localhost/base/catalogs(1)/Products""
		}
	  }
	},
	{
	  ""__metadata"": {
		""uri"": ""http://localhost/base/catalogs(2)"",
		""type"": ""TestNamespace.Catalog1""
	  },
	  ""Id"": 2,
	  ""Name"": ""Cat2"",
	  ""Products"": {
		""__deferred"": {
		  ""uri"": ""http://localhost/base/catalogs(2)/Products""
		}
	  }
	}
  ]
}");
		}

		[Test]
		public void EntitySet_WithResourceProperty_View_JSon_Success()
		{
			Process("/products/", SegmentOp.View, _model, accept: MediaTypes.JSon);

			_response.contentType.Should().Be(MediaTypes.JSon);

			_body.ToString().Should().Be(@"{
  ""d"": [
	{
	  ""__metadata"": {
		""uri"": ""http://localhost/base/products(1)"",
		""type"": ""TestNamespace.Product1""
	  },
	  ""Id"": 1,
	  ""Name"": ""Product1"",
	  ""Price"": 0.0,
	  ""Created"": ""0001-01-01T00:00:00"",
	  ""Modified"": ""0001-01-01T00:00:00"",
	  ""IsCurated"": false,
	  ""Catalog"": {
		""__deferred"": {
		  ""uri"": ""http://localhost/base/products(1)/Catalog""
		}
	  }
	},
	{
	  ""__metadata"": {
		""uri"": ""http://localhost/base/products(2)"",
		""type"": ""TestNamespace.Product1""
	  },
	  ""Id"": 2,
	  ""Name"": ""Product2"",
	  ""Price"": 0.0,
	  ""Created"": ""0001-01-01T00:00:00"",
	  ""Modified"": ""0001-01-01T00:00:00"",
	  ""IsCurated"": false,
	  ""Catalog"": {
		""__deferred"": {
		  ""uri"": ""http://localhost/base/products(2)/Catalog""
		}
	  }
	}
  ]
}");
		}

		[Test]
		public void EntitySet_WithPropMappingToTitle_View_JSon_Success()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set).
						AddAttribute(new EntityPropertyMappingAttribute("Name", SyndicationItemProperty.Title, SyndicationTextContentKind.Xhtml, false));
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});

			Process("/catalogs/", SegmentOp.View, model, accept: MediaTypes.JSon);

			_response.contentType.Should().Be(MediaTypes.JSon);
			_body.ToString().Should().Be(@"{
  ""d"": [
	{
	  ""__metadata"": {
		""uri"": ""http://localhost/base/catalogs(1)"",
		""type"": ""TestNamespace.Catalog1""
	  },
	  ""Id"": 1,
	  ""Name"": ""Cat1"",
	  ""Products"": {
		""__deferred"": {
		  ""uri"": ""http://localhost/base/catalogs(1)/Products""
		}
	  }
	},
	{
	  ""__metadata"": {
		""uri"": ""http://localhost/base/catalogs(2)"",
		""type"": ""TestNamespace.Catalog1""
	  },
	  ""Id"": 2,
	  ""Name"": ""Cat2"",
	  ""Products"": {
		""__deferred"": {
		  ""uri"": ""http://localhost/base/catalogs(2)/Products""
		}
	  }
	}
  ]
}");
		}

		[Test]
		public void EntitySet_WithComplexType_View_JSon_Success()
		{
			Process("/suppliers/", SegmentOp.View, _model, accept: MediaTypes.JSon);

			_response.contentType.Should().Be(MediaTypes.JSon);

			_body.ToString().Should().Be(@"{
  ""d"": [
	{
	  ""__metadata"": {
		""uri"": ""http://localhost/base/suppliers(1)"",
		""type"": ""TestNamespace.Supplier1""
	  },
	  ""Id"": 1,
	  ""Address"": {
		""__metadata"": {
		  ""type"": ""TestNamespace.Address1""
		},
		""Street"": ""wilson ave"",
		""Zip"": ""vxxxx"",
		""Country"": ""canada""
	  }
	},
	{
	  ""__metadata"": {
		""uri"": ""http://localhost/base/suppliers(2)"",
		""type"": ""TestNamespace.Supplier1""
	  },
	  ""Id"": 2,
	  ""Address"": {
		""__metadata"": {
		  ""type"": ""TestNamespace.Address1""
		},
		""Street"": ""kingsway ave"",
		""Zip"": ""zxxxx"",
		""Country"": ""canada""
	  }
	}
  ]
}");

		}
	}
}
