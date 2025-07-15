// src/components/ProductCard.jsx
import React from 'react';

const ProductCard = ({ product }) => {
    console.log(product)
  return (
    <div className="bg-white dark:bg-gray-800 shadow rounded-lg p-4 flex flex-col sm:flex-row gap-4">
      {/* Image */}
      <div className="w-full sm:w-40 h-40 bg-gray-200 rounded overflow-hidden flex items-center justify-center">
        {product.image_link ? (
          <img src={product.image_link} alt={product.title} className="object-cover h-full w-full" />
        ) : (
          <span className="text-gray-500">No Image</span>
        )}
      </div>

      {/* Info */}
      <div className="flex-1 space-y-2">
        <h2 className="text-xl font-bold text-gray-900 dark:text-white">{product.title}</h2>
        <p className="text-gray-600 dark:text-gray-300">{product.id}</p>
        <p className="text-gray-600 dark:text-gray-300">{product.description}</p>
        <div className="text-sm text-gray-500 dark:text-gray-400">
          <p><strong>Brand:</strong> {product.brand}</p>
          <p><strong>Color:</strong> {product.color}</p>
          <p><strong>Size:</strong> {product.size}</p>
        </div>
        <div className="text-sm mt-2">
          <p className="line-through text-red-400">{product.price}</p>
          <p className="text-green-500 font-semibold">{product.sale_price}</p>
        </div>
        {product.link && (
          <a
            href={product.link}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-block mt-2 text-blue-600 hover:underline"
          >
            View Product →
          </a>
        )}
      </div>
    </div>
  );
};

export default ProductCard;
