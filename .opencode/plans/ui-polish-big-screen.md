# UI Polish Plan: Big Screen/Projector Readability

## Summary
Scale all UI elements ~1.4-1.5x for projector/aula readability, increase contrast for gray text, and improve spacing.

## File: `Assets/Resources/UI/Common.uss`

### Font Size Changes
| Class | Current | New |
|-------|---------|-----|
| `.video-placeholder` | 14px | 22px |
| `.skip-button` | 14px | 22px |
| `.subtitle-label` | 20px | 28px |
| `.tutorial-text-line` | 16px | 24px |
| `.tutorial-button` | 16px | 24px |
| `.tutorial-title` | 28px | 36px |
| `.suspect-name-label` | 24px | 34px |
| `.close-button` | 18px | 24px |
| `.portrait-placeholder` | 14px | 22px |
| `.drug-test-button` | 14px | 22px |
| `.drug-test-result-label` | 18px | 26px |
| `.description-label` | 16px | 22px |
| `.evidence-title` | 16px | 22px |
| `.evidence-text-label` | 14px | 20px |
| `.verdict-title` | 18px | 26px |
| `.verdict-button` | 15px | 22px |
| `.result-title` | 32px | 42px |
| `.result-entry Label` | 14px | 22px |
| `.next-level-button` | 18px | 26px |
| `.check-status-title` | 24px | 34px |
| `.check-status-name` | 18px | 26px |
| `.check-status-verdict` | 16px | 22px |
| `.check-status-empty-title` | 20px | 28px |
| `.check-status-empty-message` | 14px | 22px |
| `.check-result-button` | 18px | 26px |
| `.status-hud-button` | 16px | 24px |
| `.play-button` | 24px | 28px |

### Dimension Changes
| Class | Property | Current | New |
|-------|----------|---------|-----|
| `.skip-button` | width/height | 100x36 | 150x52 |
| `.skip-button-container` | right/bottom | 30px | 40px |
| `.play-container` | gap | 20px | 28px |
| `.play-button` | width/height | 200x60 | 260x72 |
| `.tutorial-content` | width | 500px | 800px |
| `.tutorial-content` | padding | 30px | 40px |
| `.tutorial-title` | margin-bottom | 20px | 24px |
| `.tutorial-text` | gap | 10px | 14px |
| `.tutorial-button` | width/height | 180x44 | 280x64 |
| `.detail-header` | width | 700px | 1000px |
| `.detail-header` | padding | 15px 20px | 20px 30px |
| `.close-button` | width/height | 36x36 | 52x52 |
| `.detail-body` | width/min-height | 700x300 | 1000x420 |
| `.detail-left` | padding/gap | 20px/15px | 28px/20px |
| `.portrait-container` | width/height | 160x200 | 220x280 |
| `.drug-test-button` | width/height | 160x40 | 240x56 |
| `.drug-test-result-label` | min-height | 28px | 40px |
| `.detail-right` | padding/gap | 20px/15px | 28px/20px |
| `.description-label` | max-height | 120px | 160px |
| `.evidence-section` | gap/margin-top | 8px/10px | 10px/14px |
| `.evidence-text-label` | max-height | 100px | 140px |
| `.verdict-section` | width/padding/gap | 700x20x12 | 1000x28x18 |
| `.verdict-button` | width/height | 160x44 | 240x60 |
| `.results-scroll` | width/height | 600x400 | 900x500 |
| `.results-scroll` | padding | 15px | 20px |
| `.results-container` | gap | 15px | 20px |
| `.result-entry` | padding/gap | 15px/6px | 20px/10px |
| `.next-level-button` | width/height | 200x48 | 300x64 |
| `.check-status-header` | width/padding | 600px/15px 20px | 900px/20px 30px |
| `.check-status-header .close-button` | width/height | 36x36 | 52x52 |
| `.check-status-header .close-button` | font-size | 18px | 24px |
| `.check-status-scroll` | width/height | 600x350 | 900x500 |
| `.check-status-scroll` | padding | 15px | 20px |
| `.check-status-container` | gap | 12px | 18px |
| `.check-status-entry` | padding | 15px 20px | 20px 28px |
| `.check-status-empty` | padding | 40px 30px | 50px 40px |
| `.check-status-empty-message` | max-width | 500px | 700px |
| `.check-result-button` | width/height | 600x48 | 900x64 |
| `.status-hud-button` | padding | 12px 24px | 18px 36px |

### Color Contrast Changes
| Class | Property | Current | New |
|-------|----------|---------|-----|
| `.video-placeholder` | color | rgb(150,150,150) | rgb(195,195,195) |
| `.subtitle-label` | color | rgb(180,180,180) | rgb(210,210,210) |
| `.portrait-placeholder` | color | rgb(120,120,140) | rgb(175,175,190) |
| `.evidence-text-label` | color | rgb(200,200,200) | rgb(215,215,215) |
| `.check-status-verdict` | color | rgb(220,220,220) | rgb(215,215,215) |
| `.check-status-empty-title` | color | rgb(150,150,150) | rgb(195,195,195) |
| `.check-status-empty-message` | color | rgb(180,180,180) | rgb(210,210,210) |
| `.drug-test-button:disabled` | color | rgb(100,100,110) | rgb(160,160,170) |
| `.check-result-button:disabled` | color | rgb(150,150,150) | rgb(160,160,170) |
| `.hud-panel` | right/bottom | 20px | 24px |

### Border Radius Upgrades (smoother corners for larger elements)
| Class | Current | New |
|-------|---------|-----|
| `.tutorial-content` | 12px | 16px |
| `.detail-header` | 12px | 16px |
| `.detail-body` (implicit) | - | add radius |
| `.verdict-section` | 12px | 16px |
| `.portrait-container` | 8px | 10px |
| `.drug-test-button` | 6px | 8px |
| `.verdict-button` | 6px | 8px |
| `.play-button` | 8px | 10px |
| `.results-scroll` | 8px | 10px |
| `.result-entry` | 8px | 10px |
| `.check-status-header` | 12px | 16px |
| `.check-result-button` | 0 0 12px 12px | 0 0 16px 16px |

## No changes needed to UXML files
All layout structure remains the same - only CSS (Common.uss) needs updating.