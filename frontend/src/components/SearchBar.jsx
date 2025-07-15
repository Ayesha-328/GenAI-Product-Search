import React from "react";

const SearchBar = ({ query, setQuery, onSearch }) => {
  return (
   <form
  onSubmit={(e) => {
    e.preventDefault(); // prevent page reload
    onSearch();         // trigger your search function
  }}
  className="flex gap-2 items-center justify-center w-full max-w-md"
>
  <input
    type="text"
    placeholder="Search product..."
    value={query}
    onChange={(e) => setQuery(e.target.value)}
    className="border p-2 rounded w-full"
  />
  <button
    type="submit"
    className="bg-blue-500 text-white px-4 py-2 rounded"
  >
    Search
  </button>
</form>

  );
};

export default SearchBar;



// import React, { useState } from 'react';
// import ProductCard from './ProductCard';


// const SearchBar = () => {
//   const [query, setQuery] = useState('');
//   const [products, setProducts] = useState([]);
//   const [hasSearched, setHasSearched] = useState(false);

//   const handleSearch = async () => {
//     try {
//       const res = await fetch(`http://localhost:5290/api/Search?query=${encodeURIComponent(query)}`);
//       const data = await res.json();
//       setProducts(data);
//     } catch (error) {
//       console.error("Search failed:", error);
//     }
//   };

//   return (
//     <div className="p-4">
//         <div className="flex gap-2 items-center justify-center">
//       <input
//         type="text"
//         placeholder="Search product..."
//         value={query}
//         onChange={(e) => setQuery(e.target.value)}
//         className="border p-2 rounded w-full max-w-md"
//       />
//       <button
//         onClick={handleSearch}
//         className="mt-2 bg-blue-500 cursor-pointer text-white px-4 py-2 rounded"
//       >
//         Search
//       </button>

//         </div>

//       <div className="max-w-4xl mx-auto px-4 py-6">
//         {hasSearched && products.length > 0 ? (
//   <div className="mt-6 grid gap-4">
//     {products.map((product, index) => (
//       <ProductCard key={index} product={product} />
//     ))}
//   </div>
// ) : (
//   <p className="text-gray-400 mt-4">No products found.</p>
// )}


//       </div>
//     </div>
//   );
// };

// export default SearchBar;
