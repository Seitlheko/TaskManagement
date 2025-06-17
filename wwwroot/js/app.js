const API_BASE = "/api/tasks"; // Adjust if API is under a route or versioned like /api/tasks

document.addEventListener("DOMContentLoaded", () => {
    loadTasks();

    document.getElementById("taskForm").addEventListener("submit", async (e) => {
        e.preventDefault();

        const title = document.getElementById("title").value;
        const description = document.getElementById("description").value;
        const startDate = document.getElementById("startDate").value;

        const task = {
            title,
            description,
            assignedToId: 1, // Static ID for now
            status: "Pending",
            startDate,
            endDate: startDate // Use startDate as endDate for now
        };

        try {
            await fetch(API_BASE, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(task)
            });
            document.getElementById("taskForm").reset();
            loadTasks();
        } catch (err) {
            alert("Failed to create task.");
        }
    });
});

async function loadTasks() {
    try {
        const response = await fetch(API_BASE);
        const tasks = await response.json();

        const container = document.getElementById("tasksList");
        container.innerHTML = "";

        tasks.forEach(task => {
            const card = document.createElement("div");
            card.className = "col";
            card.innerHTML = `
                <div class="card h-100 shadow-sm border-primary">
                    <div class="card-body">
                        <h5 class="card-title text-primary">${task.title}</h5>
                        <p class="card-text">${task.description}</p>
                        <p class="text-muted">
                            Status: <strong>${task.status}</strong><br>
                            Start: ${new Date(task.startDate).toLocaleDateString()}
                        </p>
                        <button class="btn btn-danger btn-sm" onclick="deleteTask(${task.id})">
                            <i class="fas fa-trash-alt"></i> Delete
                        </button>
                    </div>
                </div>
            `;
            container.appendChild(card);
        });

    } catch (err) {
        console.error("Failed to load tasks", err);
        alert("Error loading tasks");
    }
}

async function deleteTask(id) {
    if (!confirm("Are you sure you want to delete this task?")) return;

    try {
        await fetch(`${API_BASE}/${id}`, {
            method: "DELETE"
        });
        loadTasks();
    } catch (err) {
        alert("Failed to delete task.");
    }
}
