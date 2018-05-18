//You can use DbContext extension CreateScalarQuery
var queryFromDbContext = myDbContext.CreateScalarQuery ((c) => c.Assignments.Count ())
    .Concat (myDbContext.CreateScalarQuery ((c) => c.Assignments.Count ()));

var xrun = queryFromDbContext.ToArray ();

//Or you can use SelectScalar extension of IQueryable<T>
var queryFromIQueryable = myDbContext.Assignments.SelectScalar (q => q.Count ())
    .Concat (myDbContext.Assignments.SelectScalar (q => q.Count ()));
xrun = queryFromIQueryable.ToArray ();