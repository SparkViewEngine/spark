Spark View Engine
=================

Spark is a view engine for [ASP.NET MVC](http://www.asp.net/mvc), [FubuMVC](http://mvc.fubu-project.org/), [NancyFx](http://nancyfx.org/) and [Castle Project MonoRail](http://www.castleproject.org/projects/monorail/) frameworks. The idea is to allow the html to dominate the flow and any code to fit seamlessly.
```html
<viewdata products="IEnumerable[[Product]]"/>
<ul if="products.Any()">
  <li each="var p in products">${p.Name}</li>
</ul>
<else>
  <p>No products available</p>
</else>
```
Although we also support "left-offset" syntax inspired by frameworks like [Jade](http://jade-lang.com/) and [Haml](http://haml.info/)
```html
viewdata products="IEnumerable[[Product]]"
ul if="products.Any()"
  li each="var p in products" 
    ${p.Name}
else
  p |No products available
```
### Getting Started

 * Firstly, check out the [documentation](https://github.com/SparkViewEngine/spark/wiki)
 * Next, you can take a look at the [community resources](https://github.com/SparkViewEngine/spark/wiki/Community-Resources) we've gathered over time 

### Installation

It's as easy as `PM> Install-Package Spark` from (nuget)[http://nuget.org/packages/Spark] for the core

### Need Help

[Google Group](https://groups.google.com/forum/?fromgroups=#!forum/spark-dev)
Twitter
[Community Resources](https://github.com/SparkViewEngine/spark/wiki/Community-Resources)

### Core Team
