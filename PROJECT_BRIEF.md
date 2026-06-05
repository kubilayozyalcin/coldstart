# Sparse Knowledge Environments için Adaptif Hibrit RAG
## Cold Start Problemine Çok Katmanlı Bir Yaklaşım

---

## Proje Bilgileri

| Alan | Bilgi |
|------|-------|
| Öğrenci | Kubilay Özyalçın |
| Öğrenci No | *********** |
| Program | Tezsiz Yüksek Lisans — Bilgisayar Mühendisliği |
| Üniversite | İstanbul Nişantaşı Üniversitesi |
| Danışman | Prof. Dr. Yaşar Hoşcan |
| Dönem | Bahar 2025-2026 |

---

## Başlık

**Türkçe:** Sparse Knowledge Environments için Adaptif Hibrit RAG: Cold Start Problemine Çok Katmanlı Bir Yaklaşım

**İngilizce:** Adaptive Hybrid RAG for Sparse Knowledge Environments: A Multi-Layer Approach to the Cold Start Problem

---

## Problem Statement

RAG (Retrieval-Augmented Generation) sistemleri, anlam tabanlı soru-cevap için vektörel bilgi tabanına ihtiyaç duyar. Ancak sistem ilk kurulduğunda vektör veritabanı boştur — embedding oluşmamıştır. Bu durumda anlam tabanlı arama yapılamaz; sistem kullanıcıya cevap veremez; onboarding süreci başarısız olur. Bu duruma **cold start problemi** denir. Mevcut RAG literatürü bu problemi sistematik olarak ele almamıştır; araştırmalar yeterli bilgi tabanı mevcudiyetini varsaymaktadır.

---

## Çözüm: 3 Katmanlı Adaptif Pipeline

### Layer 1 — Kural Tabanlı (Sparse / Sıfır Veri)
- BM25 / TF-IDF tabanlı keyword arama (BM25Sharp veya manual).
- LLM gerektirmez, deterministik.
- Eşik: `documentCount < 50`.

### Layer 2 — Hafif Parametrik Model (Orta Veri)
- OpenAI `text-embedding-3-small` + cosine similarity.
- LLM çağrısı yok; sadece embedding similarity.
- Eşik: `50 ≤ documentCount < 200`.

### Layer 3 — Tam Vektörel RAG (Yeterli Veri)
- Qdrant + OpenAI embedding + GPT-4o-mini.
- Microsoft Semantic Kernel ile orchestration.
- Klasik RAG: retrieve → augment → generate.
- Eşik: `documentCount ≥ 200`.

### Geçiş Mekanizması
Runtime'da `documentCount`'a göre otomatik, kullanıcı müdahalesi olmadan.

---

## Tech Stack

| Katman | Teknoloji |
|--------|-----------|
| Ana Dil | .NET 8 / C# |
| Web API | ASP.NET Core 8 |
| Orchestration | Microsoft Semantic Kernel (Faz 3) |
| LLM | OpenAI GPT-4o-mini |
| Embedding | OpenAI text-embedding-3-small |
| Vektör DB | Qdrant (Docker) — Faz 3 |
| Layer 1 Search | Manual BM25 (Türkçe-duyarlı tokenizer) |
| Test | xUnit |
| Demo | Swagger UI |

---

## Değerlendirme Metrikleri

- **Faithfulness** — Cevap, belgelerle tutarlı mı?
- **Answer Relevancy** — Cevap, soruyla alakalı mı?
- **Activation Time** — Sistem ilk doğru cevabı ne kadar sürede veriyor?
- **Layer Transition Accuracy** — Doğru eşikte mi geçiş yapılıyor?

---

## Özgün Katkı (Novelty)

1. Veri yoğunluğuna duyarlı **adaptif katman geçişi** — RAG literatüründe ilk kez sistematik olarak ele alınıyor.
2. Kural tabanlı + parametrik + vektörel erişimi **tek pipeline altında** birleştiren özgün mimari.
3. **.NET 8 ekosistemi** — Akademik RAG çalışmaları Python ağırlıklı; .NET katkısı ayrıca değerli.
4. Mevcut çalışmalar (ColdRAG, Self-RAG, Hybrid RAG) bağımsız geçiş mekanizması önermemiş — bu boşluk doğrudan dolduruluyor.

---

## Kaynakça

1. Lewis, P. et al. (2020). *Retrieval-Augmented Generation for Knowledge-Intensive NLP Tasks.* NeurIPS.
2. Gao, Y. et al. (2024). *Retrieval-Augmented Generation for LLMs: A Survey.* arXiv:2312.10997.
3. Asai, A. et al. (2024). *Self-RAG.* ICLR.
4. Chen, H. & Xu, H. (2025). *ColdRAG: Cold-Start Recommendation with Knowledge-Guided RAG.* arXiv:2505.20773.
5. Masood, A. (2024). *Hybrid RAG: A Comprehensive Survey.* arXiv:2410.12837.
6. Shrivastava, D. et al. (2025). *Advancing RAG for Structured Enterprise Data.* arXiv:2507.12425.
7. Wang, X. et al. (2025). *RAG and LLMs for Enterprise Knowledge Management.* Preprints, 202512.0359.
