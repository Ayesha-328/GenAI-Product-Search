import React, { useState } from "react";
import SearchBar from "../components/SearchBar";
import Results from "../components/Results";

const Home = () => {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [hasSearched, setHasSearched] = useState(false);

  const handleSearch = async () => {
    setLoading(true);
    setHasSearched(true);
    try {
      const response = await fetch(`http://localhost:5290/api/Search?query=${encodeURIComponent(query)}`);
      
      // Check for HTML response error (often due to incorrect backend URL)
      const contentType = response.headers.get("content-type");
      if (!contentType || !contentType.includes("application/json")) {
        throw new Error("Invalid response from server (not JSON)");
      }

      const data = await response.json();
      setResults(data);
    } catch (err) {
      console.error("Search failed:", err);
      setResults([]); // Clear results on error
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex flex-col items-center justify-center p-6">
      <h1 className="text-4xl font-bold mb-6">🔍 AI Search App</h1>
      <SearchBar query={query} setQuery={setQuery} onSearch={handleSearch} />
      <Results results={results} loading={loading} hasSearched={hasSearched} />
    </div>
  );
};

export default Home;
