import openai
import pandas as pd
import json
import math
from dotenv import load_dotenv
import os

# 1. Azure OpenAI Config
openai.api_type = "azure"
load_dotenv()
openai.api_base = os.getenv("OPENAI_API_BASE")
openai.api_version = os.getenv("OPENAI_API_VERSION", "2023-05-15")
openai.api_key = os.getenv("OPENAI_API_KEY")
embedding_model = "embedding-model"

# Load Excel file
df = pd.read_excel("Product Dummy Data.xlsx")

# Clean columns
exclude_columns = ['links','image_link','availability', 'availability_date','tax', 'mpn']
df = df.drop(columns=[col for col in exclude_columns if col in df.columns])

# Replace NaN with None (for safe JSON serialization)
df = df.where(pd.notnull(df), None)

# Combine relevant fields into a single searchable string
def combine_fields(row):
    parts = []
    if row.get("title"): parts.append(f"Title: {row['title']}")
    if row.get("description"): parts.append(f"Description: {row['description']}")
    if row.get("brand"): parts.append(f"Brand: {row['brand']}")
    if row.get("product_type"): parts.append(f"Type: {row['product_type']}")
    if row.get("color"): parts.append(f"Color: {row['color']}")
    if row.get("material"): parts.append(f"Material: {row['material']}")
    if row.get("size"): parts.append(f"Size: {row['size']}")
    if row.get("custom_label_0"): parts.append(f"Label0: {row['custom_label_0']}")
    if row.get("custom_label_1"): parts.append(f"Label1: {row['custom_label_1']}")
    if row.get("custom_label_2"): parts.append(f"Label2: {row['custom_label_2']}")
    if row.get("custom_label_3"): parts.append(f"Label3: {row['custom_label_3']}")
    if row.get("custom_label_4"): parts.append(f"Label4: {row['custom_label_4']}")
    return " ".join(parts)

texts = df.apply(combine_fields, axis=1)

# Generate embeddings
products = []
for idx, text in enumerate(texts):
    print(f"Embedding {idx + 1}/{len(texts)}")
    try:
        response = openai.Embedding.create(
            input=text,
            engine=embedding_model
        )
        embedding = response["data"][0]["embedding"]
        product_data = df.iloc[idx].to_dict()
        product_data["embedding"] = embedding
        products.append(product_data)
    except Exception as e:
        print(f"Error on row {idx}: {e}")

# Remove any invalid embeddings
def clean_nan(obj):
    if isinstance(obj, dict):
        return {k: clean_nan(v) for k, v in obj.items()}
    elif isinstance(obj, list):
        return [clean_nan(i) for i in obj]
    elif isinstance(obj, float) and math.isnan(obj):
        return None
    return obj

def is_valid_embedding(embedding):
    return isinstance(embedding, list) and all(isinstance(x, (float, int)) for x in embedding)

cleaned_products = []
for p in products:
    p = clean_nan(p)
    if is_valid_embedding(p.get("embedding")):
        cleaned_products.append(p)

# Save final cleaned embeddings
with open("product_embeddings_cleaned.json", "w") as f:
    json.dump(cleaned_products, f, indent=2)

print(f"✅ Saved {len(cleaned_products)} products with valid embeddings.")