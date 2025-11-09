# EduPro – Intelligent Educational Platform

EduPro is a modern educational platform for students that combines an ASP.NET Core web app with AI services to help you learn more efficiently: organize notes, extract text from images, generate quizzes, check grammar, get explanations, and manage study time.

The application runs in Docker and can be accessed at:
http://localhost:7100/

---

## About EduPro

### What problems it solves

- Scattered handwritten / photographed notes  
- Lack of time to organize and review materials  
- Difficulty understanding complex concepts  
- Frequent grammar and writing mistakes  
- Need for personalized quizzes and study tools  

### What EduPro offers

- Text extraction (OCR) from handwritten or printed notes  
- Note organization by subject/category  
- Question answering based on your own notes  
- Automatic quiz generation from any text  
- Grammar correction with explanations  
- Concept explanations at different difficulty levels  
- Pomodoro timer for focused study sessions  

---

## Main Features

### 1. Note Upload & OCR
- Upload images of notes (handwritten or printed).  
- AI extracts the text and optionally generates a short summary.  
- Extracted content is saved as a searchable note.

### 2. Note Organization by Categories
- Create subject categories (e.g., Math, Physics, CS).  
- Assign notes to categories and filter quickly.  
- Designed for working with many notes across multiple subjects.

### 3. AI Question Answering (Q&A)
- Select a category or note as context.  
- Ask natural-language questions.  
- AI answers based on your own stored notes, not generic content.

### 4. Quiz Generator
- Input or select text (e.g., a chapter or note).  
- Choose difficulty and number of questions.  
- AI generates multiple-choice questions with correct answers and explanations.

### 5. Grammar Check
- Paste or select text (essay, report, email, note).  
- AI returns a corrected version plus a list of changes.  
- Useful for academic writing and formal communication.

### 6. Concept Explanation
- Type any concept (e.g., “Fourier transform”, “entropy”, “photosynthesis”).  
- Choose the explanation level (simple, moderate, concise).  
- Get clear explanations with examples and intuitive analogies.

### 7. Pomodoro Timer
- 25-minute focus blocks, 5-minute short breaks, 15-minute long break.  
- Helps structure study sessions and reduce distractions.  

---

## Technologies Used

**Web Application**
- ASP.NET Core 8 (Razor Pages)  
- Entity Framework Core + SQLite  
- Docker  

**AI & Backend**
- Python 3.10+  
- FastAPI  
- AWS Bedrock (Claude 3.5 Sonnet) via Boto3  
- Pydantic, python-dotenv  

**Frontend**
- Bootstrap 5.3  
- Custom CSS (dark theme)  
- Bootstrap Icons  

---

## Installation and Setup

### Prerequisites

- .NET SDK 8.0+  
- Python 3.10+  
- Docker (recommended for deployment)  
- AWS Bedrock access token (for AI features)

### Quick Setup (Docker)

From the `./edupro/edupro` directory:

```bash
docker compose up --build
```

Then open:
http://localhost:7100/

### Manual Setup (Dev Mode)

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd edupro
   ```

2. Install Python dependencies:
   ```bash
   pip install -r requirements.txt
   ```

3. Configure environment variables:
   ```bash
   cp .env.example .env
   ```
   Edit `.env`:
   ```env
   AWS_BEARER_TOKEN_BEDROCK=your-aws-token-here
   ```

4. Start the AI API (Python):
   ```bash
   python main.py
   ```

5. Start the ASP.NET app:
   ```bash
   dotnet run
   ```

6. Visit:
   ```
   http://localhost:5000/
   ```

---

## Usage Guide (Typical Workflows)

- **Digitalize notes:** Upload images → OCR → Save as text notes → Organize by category.  
- **Prepare for an exam:** Select subject → Ask questions on your notes → Generate quizzes for practice.  
- **Check an assignment:** Paste your essay → Run grammar check → Apply corrections.  
- **Understand a concept:** Use Explain feature → Choose difficulty → Read tailored explanation.  
- **Focused study session:** Start Pomodoro → Study with fixed intervals → Use breaks to rest.

---

## For Developers

### Project Structure

```text
edupro/
├── Pages/               # Razor Pages (UI + handlers)
├── Models/              # EF Core models (Note, Category, etc.)
├── Data/                # AppDbContext (EF Core)
├── wwwroot/             # Static files (CSS, JS, images)
├── main.py              # Python AI/FastAPI service
├── requirements.txt     # Python dependencies
├── appsettings.json     # .NET app configuration
├── edupro.csproj        # .NET project file
└── edupro.db            # SQLite database (local)
```

### Core API Endpoints (Python / FastAPI)

- `POST /ocr` – Extract text from uploaded images  
- `POST /generate-quiz` – Generate multiple-choice questions  
- `POST /ask-question` – Answer questions using note context  
- `POST /grammar-check` – Return corrected text and corrections list  

---

## Security and Privacy

- Notes and categories are stored locally in SQLite (`edupro.db`).  
- AI calls only use temporary data for each request.  
- Secrets (AWS token, etc.) are stored in `.env` (ignored by Git).  
- Input validated in both .NET and Python components.

---

## Roadmap

- Export notes (PDF/Markdown)  
- Flashcard generation  
- Study analytics  
- Cloud sync & collaboration  
- Voice assistant and personalization  

---
