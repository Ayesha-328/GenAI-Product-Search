import React from "react";
import ProductCard from "./ProductCard";

const Results = ({ results, loading, hasSearched }) => {
  if (loading) return <p className="mt-4">Loading...</p>;

  if (hasSearched && results.length === 0)
    return <p className="mt-4">No products found</p>;

  return (
    <div className="grid gap-4 mt-4">
      {results.map((product, i) => (
        <ProductCard key={i} product={product} />
      ))}
    </div>
  );
};

export default Results;
