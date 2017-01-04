#pragma once
#include "FlashImageScene.h"

struct Person;
class RectangleGeometry;

class Box2dScene final : public Scene
{
public:
	Box2dScene(int count, int itemId, const std::vector<int>& personIds);
	virtual void CreateDeviceResources(CHwndRenderTarget * target) override;
	virtual void CreateDeviceSizeResources(CHwndRenderTarget * target) override;
	virtual void Update() override;
	virtual void Render(CHwndRenderTarget * target) override;
	virtual void KeyUp(UINT key) override;

private:
	const float RelativeSize = 10.0f;
	const float BorderWidth = RelativeSize / 100;
	const float PersonSize = 0.40f;
	const int _requiredCount;
	const int _itemId;

	enum class State
	{
		Pending,
		Started,
		Triggled,
		Completed,
	};

	std::mt19937 _rd{ std::random_device{}() };
	State _state;
	const std::vector<int> _allPersonIds;
	std::unordered_set<int> _luckyPersonIds;

	// mist
	void PreRenderBody(CHwndRenderTarget* target, b2Body * body);

	// b2 related
	std::vector<b2Body*> _borders;
	std::vector<b2Body*> _personBodies;
	b2World _world;
	int _updateCount = 0;

	b2Body* CreateBorderBody(float x, float y, float angle, float length);
	b2Body* CreatePersonBody(int personId);
	void FindLuckyPersons();
	void EnterTriggerMode();

	// dxres
	RectangleGeometry* GetOrCreateBorderGeometry(CHwndRenderTarget* target, float length);
	float _scale;
	CD2DSolidColorBrush *_borderBrush, *_luckyBrush;
	std::unordered_map<int, CD2DBitmapBrush *> _personBrushes;
	std::unordered_map<float, RectangleGeometry*> _borderGeometries;
};

class RectangleGeometry final : public CD2DGeometry
{
public:
	RectangleGeometry(CRenderTarget* target, const D2D1_RECT_F rect, bool isAutoDestroy = true);
	virtual HRESULT Create(CRenderTarget * pRenderTarget) override;

private:
	D2D1_RECT_F _rect;
};