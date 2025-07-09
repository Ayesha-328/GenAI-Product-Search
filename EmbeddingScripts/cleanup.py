import json
import math
import pandas as pd

def clean_nan(obj):
    if isinstance(obj, dict):
        return {k: clean_nan(v) for k, v in obj.items()}
    elif isinstance(obj, list):
        return [clean_nan(i) for i in obj]
    elif isinstance(obj, float) and math.isnan(obj):
        return None
    else:
        return obj

def is_valid_embedding(embedding):
    return isinstance(embedding, list) and all(isinstance(x, (float, int)) and not math.isnan(x) for x in embedding)

# Load original file
with open("product_embeddings.json", "r") as f:
    products = json.load(f)

# Clean and filter
cleaned = []
for p in products:
    p = clean_nan(p)
    if is_valid_embedding(p.get("embedding")):
        cleaned.append(p)

# Save final clean version
with open("product_embeddings_cleaned.json", "w") as f:
    json.dump(cleaned, f, indent=2)

print(f"✅ Cleaned {len(products) - len(cleaned)} invalid entries and replaced NaN with null.")
