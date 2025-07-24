using YoutubeDownloader.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<YoutubeService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Serve the HTML page
app.MapGet("/", () => Results.Content(GetIndexHtml(), "text/html"));

app.Run();

static string GetIndexHtml()
{
    return """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>YouTube Downloader</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        
        .container {
            background: white;
            padding: 40px;
            border-radius: 20px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
            max-width: 600px;
            width: 100%;
        }
        
        h1 {
            text-align: center;
            color: #333;
            margin-bottom: 30px;
            font-size: 2.5em;
        }
        
        .input-group {
            margin-bottom: 20px;
        }
        
        label {
            display: block;
            margin-bottom: 8px;
            color: #555;
            font-weight: 600;
        }
        
        input[type="url"] {
            width: 100%;
            padding: 15px;
            border: 2px solid #ddd;
            border-radius: 10px;
            font-size: 16px;
            transition: border-color 0.3s;
        }
        
        input[type="url"]:focus {
            outline: none;
            border-color: #667eea;
        }
        
        .quality-group {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
            gap: 10px;
            margin-bottom: 20px;
        }
        
        .quality-option {
            display: flex;
            align-items: center;
            padding: 10px;
            border: 2px solid #ddd;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.3s;
        }
        
        .quality-option:hover {
            border-color: #667eea;
            background-color: #f8f9ff;
        }
        
        .quality-option input[type="radio"] {
            margin-right: 8px;
        }
        
        .quality-option input[type="radio"]:checked + label {
            color: #667eea;
            font-weight: bold;
        }
        
        button {
            width: 100%;
            padding: 15px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 10px;
            font-size: 18px;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s;
        }
        
        button:hover {
            transform: translateY(-2px);
        }
        
        button:disabled {
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }
        
        .loading {
            display: none;
            text-align: center;
            margin-top: 20px;
        }
        
        .spinner {
            border: 4px solid #f3f3f3;
            border-top: 4px solid #667eea;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
            margin: 0 auto 10px;
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        
        .error {
            background-color: #ffe6e6;
            color: #d32f2f;
            padding: 15px;
            border-radius: 8px;
            margin-top: 20px;
            display: none;
        }
        
        .success {
            background-color: #e8f5e8;
            color: #2e7d32;
            padding: 15px;
            border-radius: 8px;
            margin-top: 20px;
            display: none;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>🎬 YouTube Downloader</h1>
        
        <form id="downloadForm">
            <div class="input-group">
                <label for="url">YouTube URL:</label>
                <input type="url" id="url" placeholder="https://www.youtube.com/watch?v=..." required>
            </div>
            
            <div class="input-group">
                <label>Quality:</label>
                <div class="quality-group">
                    <div class="quality-option">
                        <input type="radio" id="highest" name="quality" value="highest" checked>
                        <label for="highest">Highest</label>
                    </div>
                    <div class="quality-option">
                        <input type="radio" id="high" name="quality" value="high">
                        <label for="high">High</label>
                    </div>
                    <div class="quality-option">
                        <input type="radio" id="medium" name="quality" value="medium">
                        <label for="medium">Medium</label>
                    </div>
                    <div class="quality-option">
                        <input type="radio" id="low" name="quality" value="low">
                        <label for="low">Low</label>
                    </div>
                </div>
            </div>
            
            <button type="submit" id="downloadBtn">Download Video</button>
        </form>
        
        <div class="loading" id="loading">
            <div class="spinner"></div>
            <p>Processing your request...</p>
        </div>
        
        <div class="error" id="error"></div>
        <div class="success" id="success"></div>
    </div>

    <script>
        document.getElementById('downloadForm').addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const url = document.getElementById('url').value;
            const quality = document.querySelector('input[name="quality"]:checked').value;
            const button = document.getElementById('downloadBtn');
            const loading = document.getElementById('loading');
            const error = document.getElementById('error');
            const success = document.getElementById('success');
            
            // Reset states
            error.style.display = 'none';
            success.style.display = 'none';
            loading.style.display = 'block';
            button.disabled = true;
            
            try {
                const response = await fetch('/api/youtube/download', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ url, quality })
                });
                
                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || 'Download failed');
                }
                
                // Get the filename from the response headers
                const contentDisposition = response.headers.get('Content-Disposition');
                let filename = 'video.mp4';
                if (contentDisposition) {
                    const filenameMatch = contentDisposition.match(/filename="(.+)"/);
                    if (filenameMatch) {
                        filename = filenameMatch[1];
                    }
                }
                
                // Create download link
                const blob = await response.blob();
                const downloadUrl = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = downloadUrl;
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(downloadUrl);
                document.body.removeChild(a);
                
                success.textContent = 'Video downloaded successfully!';
                success.style.display = 'block';
                
            } catch (err) {
                error.textContent = err.message;
                error.style.display = 'block';
            } finally {
                loading.style.display = 'none';
                button.disabled = false;
            }
        });
    </script>
</body>
</html>
""";
}
